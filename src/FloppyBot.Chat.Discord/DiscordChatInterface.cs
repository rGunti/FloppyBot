using System.Collections.Concurrent;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using FloppyBot.Chat.Discord.Config;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Registry;
using FloppyBot.Commands.Registry.Entities;
using FloppyBot.Version;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Chat.Discord;

public class DiscordChatInterface : IChatInterface
{
    public const string IF_NAME = "Discord";

    private const string SLASH_COMMAND_PREFIX = "";

    private static readonly IEmote ReadEmote = new Emoji("✅");

    private readonly ILogger<DiscordSocketClient> _clientLogger;

    private readonly IDistributedCommandRegistry _commandRegistry;
    private readonly DiscordConfiguration _configuration;
    private readonly DiscordSocketClient _discordClient;

    private readonly ILogger<DiscordChatInterface> _logger;

    private readonly ConcurrentDictionary<string, SocketSlashCommand> _slashCommandExecutions = new();

    public DiscordChatInterface(
        ILogger<DiscordChatInterface> logger,
        // ReSharper disable once ContextualLoggerProblem
        ILogger<DiscordSocketClient> clientLogger,
        DiscordConfiguration configuration,
        DiscordSocketClient discordClient,
        IDistributedCommandRegistry commandRegistry)
    {
        _logger = logger;
        _clientLogger = clientLogger;
        _configuration = configuration;
        _discordClient = discordClient;
        _commandRegistry = commandRegistry;

        _discordClient.Log += DiscordClientOnLog;
        _discordClient.Ready += DiscordClientOnReady;
        _discordClient.MessageReceived += DiscordClientOnMessageReceived;
        _discordClient.SlashCommandExecuted += DiscordClientSlashCommandExecuted;
        _discordClient.ApplicationCommandCreated += (c) =>
        {
            _logger.LogDebug(
                "Registered new command {CommandName} [{CommandId}]",
                c.Name,
                c.Id);
            return Task.CompletedTask;
        };
        _discordClient.ApplicationCommandUpdated += (c) =>
        {
            _logger.LogDebug(
                "Updated existing command {CommandName} [{CommandId}]",
                c.Name,
                c.Id);
            return Task.CompletedTask;
        };
        _discordClient.ApplicationCommandDeleted += (c) =>
        {
            _logger.LogDebug(
                "Deleted existing command {CommandName} [{CommandId}]",
                c.Name,
                c.Id);
            return Task.CompletedTask;
        };
    }

    public string Name => IF_NAME;

    public ChatInterfaceFeatures SupportedFeatures
        => ChatInterfaceFeatures.MarkdownText | ChatInterfaceFeatures.Newline;

    public string ConnectUrl
        => $"https://discordapp.com/oauth2/authorize?client_id={_configuration.ClientId}&scope=bot&permissions={_configuration.ClientId}";

    public void Connect()
    {
        _logger.LogInformation("Connecting to Discord ...");
        ConnectAsync();
    }

    public void Disconnect()
    {
        _logger.LogInformation("Disconnecting from Discord ...");
        DisconnectAsync();
    }

    public void SendMessage(ChatMessageIdentifier referenceMessage, string message)
    {
        if (_slashCommandExecutions.ContainsKey(referenceMessage.MessageId)
            && _slashCommandExecutions.Remove(referenceMessage.MessageId, out var socketSlashCommand))
        {
            socketSlashCommand.FollowupAsync(message);
            return;
        }

        var channel = _discordClient.GetChannel(referenceMessage);
        if (channel == null)
        {
            _logger.LogWarning("Could not find channel with ID {ChannelId}", referenceMessage.Channel);
            return;
        }

        if (channel is ITextChannel textChannel)
        {
            textChannel.SendMessageAsync(
                message,
                messageReference: referenceMessage.ToMessageReference());
        }
        else
        {
            _logger.LogWarning(
                "Channel found is not a text channel, cannot send message (was {ChannelType})",
                channel.GetType());
        }
    }

    public event ChatMessageReceivedDelegate? MessageReceived;

    public void Dispose()
    {
        _logger.LogTrace("Disposing interface ...");
        _discordClient.MessageReceived -= DiscordClientOnMessageReceived;
    }

    private async void ConnectAsync()
    {
        _logger.LogTrace("Logging in with Bot Token ...");
        await _discordClient.LoginAsync(TokenType.Bot, _configuration.Token);

        _logger.LogTrace("Starting Discord Connection ...");
        await _discordClient.StartAsync();
    }

    private async void DisconnectAsync()
    {
        await _discordClient.SetStatusAsync(UserStatus.Offline);
        await _discordClient.LogoutAsync();
    }

    private async Task DiscordClientOnReady()
    {
        _logger.LogInformation("Connected!");
        await SetupSlashCommands();
        await _discordClient.SetStatusAsync(UserStatus.Online);
        await _discordClient.SetGameAsync($"FloppyBot v{AboutThisApp.Info.Version}");
        _logger.LogInformation("Connect using this URL: {ConnectUrl}", ConnectUrl);
    }

    private async Task SetupSlashCommands()
    {
        _logger.LogDebug("Setting up slash commands");

        var slashCommands = _commandRegistry
            .GetAllCommands()
            .Where(c => c.AvailableOnInterfaces.Length == 0 || c.AvailableOnInterfaces.Contains(IF_NAME))
            .Where(c => !c.Hidden)
            .Select(c =>
            {
                _logger.LogTrace(
                    "Building slash command for {CommandName}",
                    c.Name);
                var description = (c.Description ??
                                   "No description was provided for this command, but I'm sure it's lovely");
                if (description.Length >= 100)
                {
                    description = description[..96] + "...";
                }

                var cmd = new SlashCommandBuilder()
                    .WithName($"{SLASH_COMMAND_PREFIX}{c.Name}")
                    .WithDescription(description);

                if (c.MinPrivilegeLevel != null)
                {
                    cmd = cmd
                        .WithDefaultMemberPermissions(ConvertToGuildPermission(c.MinPrivilegeLevel));
                }

                if (!c.NoParameters && c.Parameters.Length == 0)
                {
                    cmd = cmd
                        .AddOption(
                            "arguments",
                            ApplicationCommandOptionType.String,
                            "Additional arguments for the command (depends on the command used)",
                            isRequired: false);
                }
                else if (!c.NoParameters)
                {
                    cmd.AddOptions(
                        c.Parameters
                            .OrderBy(p => p.Order)
                            .Select(p =>
                            {
                                var cmdParam = new SlashCommandOptionBuilder()
                                    .WithName(p.Name.ToLowerInvariant())
                                    .WithDescription(p.Description ?? "An undocumented parameter")
                                    .WithType(ConvertParamType(p.Type))
                                    .WithRequired(p.Required);
                                if (p.Type == CommandParameterAbstractType.Enum && p.PossibleValues != null)
                                {
                                    foreach (var possibleValue in p.PossibleValues)
                                    {
                                        cmdParam = cmdParam
                                            .AddChoice(possibleValue, possibleValue);
                                    }
                                }

                                return cmdParam;
                            })
                            .ToArray());
                }

                return cmd;
            })
            .Select(command => command.Build())
            .ToArray();

        try
        {
            await _discordClient.BulkOverwriteGlobalApplicationCommandsAsync(
                slashCommands);
        }
        catch (HttpException ex)
        {
            _logger.LogError(ex, "Failed to setup slash command due to an exception");
        }
    }

    private static ApplicationCommandOptionType ConvertParamType(
        CommandParameterAbstractType commandParameterAbstractType)
    {
        return commandParameterAbstractType switch
        {
            CommandParameterAbstractType.String => ApplicationCommandOptionType.String,
            CommandParameterAbstractType.Enum => ApplicationCommandOptionType.String,
            CommandParameterAbstractType.Number => ApplicationCommandOptionType.Number,
            _ => throw new ArgumentOutOfRangeException(nameof(commandParameterAbstractType),
                commandParameterAbstractType, "This value is not supported!"),
        };
    }

    private Task DiscordClientOnLog(LogMessage arg)
    {
        if (arg.Exception != null)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _clientLogger.Log(TranslateLogLevel(arg.Severity), arg.Exception, arg.Message);
        }
        else
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _clientLogger.Log(TranslateLogLevel(arg.Severity), arg.Message);
        }

        return Task.CompletedTask;
    }

    private Task DiscordClientOnMessageReceived(SocketMessage socketMessage)
    {
        if (socketMessage.Author.IsBot)
        {
            _logger.LogTrace("Received message from a bot, ignoring");
            return Task.CompletedTask;
        }

        _logger.LogTrace("Received message from {Username}@{Channel}: {Message}",
            socketMessage.Author.ToString(),
            socketMessage.Channel.Name,
            socketMessage.Content);

        var message = new ChatMessage(
            NewChatMessageIdentifier(socketMessage.Channel.Id, socketMessage.Id),
            new ChatUser(
                new ChannelIdentifier(
                    IF_NAME,
                    $"{socketMessage.Author.Id}"),
                socketMessage.Author.Username,
                DeterminePrivilegeLevel(socketMessage.Author)),
            SharedEventTypes.CHAT_MESSAGE,
            socketMessage.Content,
            null,
            SupportedFeatures);

        MessageReceived?.Invoke(this, message);

        return Task.CompletedTask;
    }

    private static PrivilegeLevel DeterminePrivilegeLevel(SocketUser user)
    {
        if (user.IsBot || user.IsWebhook)
            return PrivilegeLevel.Unknown;
        if (user is SocketGuildUser guildUser)
        {
            var guildPermissions = guildUser.GuildPermissions;
            if (guildPermissions.Administrator)
                return PrivilegeLevel.Administrator;
            if (guildPermissions.ManageChannels)
                return PrivilegeLevel.Moderator;
            return PrivilegeLevel.Viewer;
        }

        return PrivilegeLevel.Unknown;
    }

    private static GuildPermission? ConvertToGuildPermission(PrivilegeLevel? level)
    {
        if (level == null)
        {
            return null;
        }

        return level switch
        {
            PrivilegeLevel.Administrator => GuildPermission.Administrator,
            PrivilegeLevel.Moderator => GuildPermission.ManageChannels,
            PrivilegeLevel.Viewer => GuildPermission.SendMessages,
            _ => null,
        };
    }

    private ChatMessageIdentifier NewChatMessageIdentifier(
        ulong channelId,
        ulong messageId)
    {
        return new ChatMessageIdentifier(
            IF_NAME,
            $"{channelId}",
            $"{messageId}");
    }

    private static LogLevel TranslateLogLevel(LogSeverity severity)
    {
        switch (severity)
        {
            case LogSeverity.Critical:
                return LogLevel.Critical;
            case LogSeverity.Error:
                return LogLevel.Error;
            case LogSeverity.Warning:
                return LogLevel.Warning;
            case LogSeverity.Info:
                return LogLevel.Information;
            case LogSeverity.Verbose:
                return LogLevel.Trace;
            case LogSeverity.Debug:
                return LogLevel.Debug;
            default:
                return LogLevel.Trace;
        }
    }

    private Task DiscordClientSlashCommandExecuted(SocketSlashCommand arg)
    {
        if (arg.User.IsBot)
        {
            _logger.LogTrace("Received command from a bot, ignoring");
            return Task.CompletedTask;
        }

        _logger.LogTrace(
            "Received command from {Username}@{Channel}: CommandName={CommandName} CommandArgs={CommandArguments}",
            arg.User.ToString(),
            arg.Channel.Name,
            arg.Data.Name,
            arg.Data.Options);

        var commandName = arg.Data.Name
            .Substring(SLASH_COMMAND_PREFIX.Length);

        var message = new ChatMessage(
            NewChatMessageIdentifier(arg.Channel.Id, arg.Id),
            new ChatUser(
                new ChannelIdentifier(
                    IF_NAME,
                    $"{arg.User.Id}"),
                arg.User.Username,
                DeterminePrivilegeLevel(arg.User)),
            SharedEventTypes.CHAT_MESSAGE,
            string.Join(' ', Enumerable.Empty<string>()
                .Append($"-{commandName}") // TODO: configurable command prefix
                .Concat(arg.Data.Options
                    .Select(o => o.Value))),
            null,
            SupportedFeatures);

        MessageReceived?.Invoke(this, message);
        _slashCommandExecutions.TryAdd($"{arg.Id}", arg);
        return arg.DeferAsync();
    }
}
