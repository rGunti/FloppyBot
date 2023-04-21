using System.Collections.Immutable;
using FloppyBot.Base.EquatableCollections;

namespace FloppyBot.Chat.Entities.Identifiers;

/// <summary>
/// An identifier used to link to channels
/// </summary>
/// <param name="Interface"></param>
/// <param name="Channel"></param>
public record ChannelIdentifier(string Interface, string Channel)
{
    public static implicit operator string(ChannelIdentifier identifier) => identifier.ToString();

    public static implicit operator ChannelIdentifier(string channelId) =>
        channelId.ParseAsChannelId();

    public override string ToString()
    {
        return IdentifierUtils.GenerateId(Interface, Channel);
    }
}

public record ExtendedChannelIdentifier(
    string Interface,
    string Channel,
    IImmutableList<string> AdditionalInfo
) : ChannelIdentifier(Interface, Channel)
{
    public ExtendedChannelIdentifier(
        string @interface,
        string channel,
        params string[] additionalInfo
    )
        : this(@interface, channel, additionalInfo.ToImmutableListWithValueSemantics()) { }

    public ExtendedChannelIdentifier(
        string @interface,
        string channel,
        IEnumerable<string> additionalInfo
    )
        : this(@interface, channel, additionalInfo.ToImmutableListWithValueSemantics()) { }

    public static implicit operator string(ExtendedChannelIdentifier identifier) =>
        identifier.ToString();

    public static implicit operator ExtendedChannelIdentifier(string channelId) =>
        channelId.ParseAsExtendedChannelId();

    public override string ToString()
    {
        return IdentifierUtils.GenerateId(new[] { Interface, Channel }.Concat(AdditionalInfo));
    }
}
