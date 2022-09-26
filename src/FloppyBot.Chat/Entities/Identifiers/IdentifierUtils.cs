namespace FloppyBot.Chat.Entities.Identifiers;

public static class IdentifierUtils
{
    public static ChatMessageIdentifier ParseAsMessageId(this string id)
    {
        string[] split = id.Split('/');
        if (split.Length < 3)
        {
            throw new ArgumentException("Provided message identifier is invalid, not enough elements found",
                nameof(id));
        }

        return new ChatMessageIdentifier(
            split[0],
            split[1],
            split[2]);
    }

    public static ChannelIdentifier ParseAsChannelId(this string id)
    {
        string[] split = id.Split('/');
        if (split.Length < 2)
        {
            throw new ArgumentException("Provided channel identifier is invalid, not enough elements found",
                nameof(id));
        }

        return new ChannelIdentifier(
            split[0],
            split[1]);
    }

    public static string GenerateId(params string[] elements)
    {
        return string.Join('/', elements);
    }
}
