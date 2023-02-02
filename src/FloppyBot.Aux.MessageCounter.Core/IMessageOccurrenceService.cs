using FloppyBot.Chat.Entities;

namespace FloppyBot.Aux.MessageCounter.Core;

public interface IMessageOccurrenceService
{
    void StoreMessage(ChatMessage chatMessage);

    int GetMessageCountInChannel(
        string channelId,
        TimeSpan maxTimeAgo);
}

