using FloppyBot.Chat.Entities.Identifiers;

namespace FloppyBot.Chat;

public interface IChatInterface : IDisposable
{
    string Name { get; }
    ChatInterfaceFeatures SupportedFeatures { get; }

    void Connect();
    void Disconnect();

    void SendMessage(string message);
    void SendMessage(ChannelIdentifier channel, string message);
    void SendMessage(ChatMessageIdentifier referenceMessage, string message);

    event ChatMessageReceivedDelegate MessageReceived;
}
