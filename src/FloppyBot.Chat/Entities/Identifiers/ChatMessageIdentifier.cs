namespace FloppyBot.Chat.Entities.Identifiers;

/// <summary>
/// An identifier used to identify to a single message
/// </summary>
/// <param name="Interface"></param>
/// <param name="Channel"></param>
/// <param name="MessageId"></param>
public record ChatMessageIdentifier(string Interface, string Channel, string MessageId)
{
    public const string NEW_MESSAGE_ID = "new";

    public static implicit operator string(ChatMessageIdentifier identifier) =>
        identifier.ToString();

    public static implicit operator ChatMessageIdentifier(string messageId) =>
        messageId.ParseAsMessageId();

    public static ChatMessageIdentifier NewFor(string channelId)
    {
        return $"{channelId}/{NEW_MESSAGE_ID}";
    }

    public override string ToString()
    {
        return IdentifierUtils.GenerateId(Interface, Channel, MessageId);
    }
}
