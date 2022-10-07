using System.Collections.Concurrent;
using Discord;
using Discord.WebSocket;
using FloppyBot.Chat.Discord.Config;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Chat.Discord;

public class DiscordChatInterface : IChatInterface
{
    public const string IF_NAME = "Discord";
    private readonly ILogger<DiscordSocketClient> _clientLogger;
    private readonly DiscordConfiguration _configuration;
    private readonly DiscordSocketClient _discordClient;

    private readonly ConcurrentDictionary<ChatMessageIdentifier, SocketMessage> _discordMessageRef = new();

    private readonly ILogger<DiscordChatInterface> _logger;

    public DiscordChatInterface(
        ILogger<DiscordChatInterface> logger,
        // ReSharper disable once ContextualLoggerProblem
        ILogger<DiscordSocketClient> clientLogger,
        DiscordConfiguration configuration,
        DiscordSocketClient discordClient)
    {
        _logger = logger;
        _clientLogger = clientLogger;
        _configuration = configuration;
        _discordClient = discordClient;

        _discordClient.Log += DiscordClientOnLog;
        _discordClient.Ready += DiscordClientOnReady;
        _discordClient.MessageReceived += DiscordClientOnMessageReceived;
    }

    public string ConnectUrl
        =>
            $"https://discordapp.com/oauth2/authorize?client_id={_configuration.ClientId}&scope=bot&permissions={_configuration.ClientId}";

    public string Name => IF_NAME;

    public ChatInterfaceFeatures SupportedFeatures =>
        ChatInterfaceFeatures.MarkdownText | ChatInterfaceFeatures.Newline;

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

    public void SendMessage(string message)
    {
        _logger.LogCritical("Cannot send message without a target!");
        throw new InvalidOperationException("Cannot send message without a target!");
    }

    public void SendMessage(ChannelIdentifier channel, string message)
    {
        throw new NotImplementedException();
    }

    public void SendMessage(ChatMessageIdentifier referenceMessage, string message)
    {
        if (!_discordMessageRef.ContainsKey(referenceMessage))
        {
            // TODO: Check what to do in this case
            return;
        }

        if (!_discordMessageRef.Remove(referenceMessage, out var msg))
        {
            _logger.LogWarning(
                "Did not find a reference message {ChatMessageId}",
                referenceMessage);
            return;
        }

        ;

        if (msg is IUserMessage userMessage)
        {
            userMessage.ReplyAsync(message);
        }
        else
        {
            _logger.LogWarning(
                "Didn't know what to do with {ChatMessageId}, message type was {MessageType}",
                referenceMessage,
                msg.GetType());
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
        await _discordClient.SetStatusAsync(UserStatus.Online);
        await _discordClient.SetGameAsync("FloppyBot v2");
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
            NewChatMessageIdentifier(socketMessage.Channel.Id),
            new ChatUser(
                new ChannelIdentifier(
                    IF_NAME,
                    $"{socketMessage.Author.Id}"),
                socketMessage.Author.Username,
                DeterminePrivilegeLevel(socketMessage.Author)),
            SharedEventTypes.CHAT_MESSAGE,
            socketMessage.Content);

        _discordMessageRef.AddOrUpdate(message.Identifier, socketMessage, (_, _) => socketMessage);

        MessageReceived?.Invoke(this, message);
        return Task.CompletedTask;
    }

    private PrivilegeLevel DeterminePrivilegeLevel(SocketUser user)
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

    private ChatMessageIdentifier NewChatMessageIdentifier(
        ulong channelId)
    {
        return new ChatMessageIdentifier(
            IF_NAME,
            $"{channelId}",
            Guid.NewGuid().ToString());
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
}
