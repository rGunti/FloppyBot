using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;

namespace FloppyBot.Chat.Mock;

public class MockChatInterface : IChatInterface
{
    public const string IF_NAME = "Mock";
    private readonly string _channelName;

    private readonly Stack<ChatMessage> _receivedMessages = new();
    private readonly Stack<string> _sentMessages = new();

    public MockChatInterface(
        ChatInterfaceFeatures features = ChatInterfaceFeatures.None,
        string channelName = "MyChannel"
    )
    {
        SupportedFeatures = features;
        MessageReceived += OnMessageReceived;
        _channelName = channelName;
    }

    public IEnumerable<ChatMessage> ReceivedMessages => _receivedMessages;
    public Stack<string> SentMessages => _sentMessages;

    public string Name => IF_NAME;
    public ChatInterfaceFeatures SupportedFeatures { get; }

    private ChannelIdentifier ChannelIdentifier => new(Name, _channelName);

    public void Connect()
    {
        // but nothing happened
    }

    public void Disconnect()
    {
        // but nothing happened
    }

    public void SendMessage(string message)
    {
        _sentMessages.Push(message);
    }

    public void SendMessage(ChannelIdentifier _, string message)
    {
        SendMessage(message);
    }

    public void SendMessage(ChatMessageIdentifier _, string message)
    {
        SendMessage(message);
    }

    public event ChatMessageReceivedDelegate? MessageReceived;

    public void Dispose()
    {
        // but nothing happened
    }

    public void InvokeReceivedMessage(
        string username,
        string message,
        PrivilegeLevel privilegeLevel
    )
    {
        var msg = new ChatMessage(
            CreateNewIdentifier(),
            new ChatUser(new ChannelIdentifier(IF_NAME, username), username, privilegeLevel),
            SharedEventTypes.CHAT_MESSAGE,
            message
        );
        MessageReceived?.Invoke(this, msg);
    }

    private ChatMessageIdentifier CreateNewIdentifier()
    {
        return new ChatMessageIdentifier(Name, _channelName, Guid.NewGuid().ToString());
    }

    private void OnMessageReceived(IChatInterface _, ChatMessage chatMessage)
    {
        _receivedMessages.Push(chatMessage);
    }
}
