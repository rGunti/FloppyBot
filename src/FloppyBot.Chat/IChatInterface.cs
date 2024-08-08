using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;

namespace FloppyBot.Chat;

public interface IChatInterface : IDisposable
{
    string Name { get; }
    ChatInterfaceFeatures SupportedFeatures { get; }

    void Connect();
    void Disconnect();

    void SendMessage(ChatMessageIdentifier referenceMessage, string message);

    bool CanHandleMessageAsResponse(ChatMessage message)
    {
        return message.Identifier.Interface == Name;
    }

    event ChatMessageReceivedDelegate MessageReceived;
}
