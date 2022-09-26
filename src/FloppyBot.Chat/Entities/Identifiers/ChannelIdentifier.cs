namespace FloppyBot.Chat.Entities.Identifiers;

/// <summary>
/// An identifier used to link to channels
/// </summary>
/// <param name="Interface"></param>
/// <param name="Channel"></param>
public record ChannelIdentifier(
    string Interface,
    string Channel)
{
    public static implicit operator string(ChannelIdentifier identifier) => identifier.ToString();

    public static implicit operator ChannelIdentifier(string channelId) => channelId.ParseAsChannelId();

    public override string ToString()
    {
        return IdentifierUtils.GenerateId(Interface, Channel);
    }
}
