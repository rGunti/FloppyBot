using Discord;
using Discord.WebSocket;
using FloppyBot.Chat.Entities.Identifiers;

namespace FloppyBot.Chat.Discord;

internal static class IdentifierExtensions
{
    public static SocketChannel? GetChannel(
        this DiscordSocketClient client,
        ChatMessageIdentifier identifier
    )
    {
        return !ulong.TryParse(identifier.Channel, out var channelId)
            ? null
            : client.GetChannel(channelId);
    }

    public static MessageReference? ToMessageReference(this ChatMessageIdentifier id)
    {
        ulong? msgRefId = null;
        if (id.MessageId == string.Empty || id.MessageId == ChatMessageIdentifier.NEW_MESSAGE_ID)
        {
            return null;
        }

        if (ulong.TryParse(id.MessageId, out var msgRefIdVal))
        {
            msgRefId = msgRefIdVal;
        }

        return new MessageReference(messageId: msgRefId, failIfNotExists: false);
    }
}
