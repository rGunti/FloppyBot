using Discord;
using Discord.Rest;
using Discord.WebSocket;
using FloppyBot.Chat.Discord.Config;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Chat.Discord;

public class DiscordChatInterface : IChatInterface
{
    public const string IF_NAME = "Discord";

    public const string EVENT_MESSAGE = "ChatMessage";

    private readonly ILogger<DiscordChatInterface> _logger;
    private readonly ILogger<DiscordSocketClient> _clientLogger;
    private readonly DiscordConfiguration _configuration;
    private readonly DiscordSocketClient _discordClient;

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

    public string Name => IF_NAME;

    public ChatInterfaceFeatures SupportedFeatures =>
        ChatInterfaceFeatures.MarkdownText | ChatInterfaceFeatures.Newline;
    
    public string ConnectUrl
        => $"https://discordapp.com/oauth2/authorize?client_id={_configuration.ClientId}&scope=bot&permissions={_configuration.ClientId}";

    public void Connect()
    {
        _logger.LogInformation("Connecting to Discord ...");
        ConnectAsync();
    }

    private async void ConnectAsync()
    {
        _logger.LogTrace("Logging in with Bot Token ...");
        await _discordClient.LoginAsync(TokenType.Bot, _configuration.Token);

        _logger.LogTrace("Starting Discord Connection ...");
        await _discordClient.StartAsync();
    }

    public void Disconnect()
    {
        _logger.LogInformation("Disconnecting from Discord ...");
        DisconnectAsync();
    }

    private async void DisconnectAsync()
    {
        await _discordClient.SetStatusAsync(UserStatus.Offline);
        await _discordClient.LogoutAsync();
    }

    public void SendMessage(string message)
    {
        throw new NotImplementedException();
    }

    public void SendMessage(ChannelIdentifier channel, string message)
    {
        throw new NotImplementedException();
    }

    public event ChatMessageReceivedDelegate? MessageReceived;

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
                PrivilegeLevel.Unknown),
            EVENT_MESSAGE,
            socketMessage.Content);
        
        MessageReceived?.Invoke(this, message);
        return Task.CompletedTask;
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

    public void Dispose()
    {
        _logger.LogTrace("Disposing interface ...");

        _discordClient.MessageReceived -= DiscordClientOnMessageReceived;
    }
}
