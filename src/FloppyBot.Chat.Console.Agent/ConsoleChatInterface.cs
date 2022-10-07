using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Chat.Console.Agent;

internal class ConsoleChatInterface : IChatInterface
{
    private const string IF_NAME = "Console";

    private readonly Thread _consoleInputThread;

    private readonly ILogger<ConsoleChatInterface> _logger;
    private readonly ConsoleAgentUserConfiguration _userConfig;

    private bool _active;

    public ConsoleChatInterface(
        ILogger<ConsoleChatInterface> logger,
        ConsoleAgentUserConfiguration userConfig)
    {
        _userConfig = userConfig;
        _logger = logger;

        _consoleInputThread = new Thread(ConsoleLoop);
    }

    public string Name => IF_NAME;
    public ChatInterfaceFeatures SupportedFeatures => ChatInterfaceFeatures.None;

    public void Connect()
    {
        _logger.LogInformation("Configured console user as {@UserConfig}", _userConfig);
        _consoleInputThread.Start();

        var task = new Task(ConsoleLoop);
        task.Start();
    }

    public void Disconnect()
    {
        _logger.LogInformation("Disconnecting ...");
        _active = false;

        _consoleInputThread.Join(TimeSpan.FromSeconds(1));
        _logger.LogInformation("Disconnected");
    }

    public void SendMessage(string message)
    {
        _logger.LogInformation("Message: {Message}", message);
    }

    public void SendMessage(ChannelIdentifier channel, string message)
    {
        SendMessage(message);
    }

    public void SendMessage(ChatMessageIdentifier referenceMessage, string message)
    {
        SendMessage(message);
    }

    public event ChatMessageReceivedDelegate? MessageReceived;

    public void Dispose()
    {
        if (_consoleInputThread.IsAlive)
        {
        }
    }

    private void ConsoleLoop()
    {
        _logger.LogInformation("Console ready! Enter a message and send using [ENTER]");
        _active = true;

        while (_active)
        {
            string? chatMessage = System.Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(chatMessage))
            {
                MessageReceived?.Invoke(
                    this,
                    CreateMessage(chatMessage));
            }
        }

        _logger.LogInformation("Console is disconnected");
    }

    private ChatMessage CreateMessage(string message)
    {
        return new ChatMessage(
            new ChatMessageIdentifier(
                IF_NAME,
                "Main",
                Guid.NewGuid().ToString()),
            new ChatUser(
                new ChannelIdentifier(
                    IF_NAME,
                    _userConfig.Username),
                _userConfig.Username,
                _userConfig.PrivilegeLevel),
            SharedEventTypes.CHAT_MESSAGE,
            message);
    }
}
