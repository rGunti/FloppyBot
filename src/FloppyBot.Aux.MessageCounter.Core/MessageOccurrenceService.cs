using FloppyBot.Aux.MessageCounter.Core.Entities;
using FloppyBot.Base.Clock;
using FloppyBot.Base.Storage;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;

namespace FloppyBot.Aux.MessageCounter.Core;

public class MessageOccurrenceService : IMessageOccurrenceService
{
    private readonly IRepository<MessageOccurrence> _repository;
    private readonly ITimeProvider _timeProvider;

    public MessageOccurrenceService(
        IRepositoryFactory repositoryFactory,
        ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _repository = repositoryFactory.GetRepository<MessageOccurrence>();
    }

    public void StoreMessage(ChatMessage chatMessage)
    {
        _repository.Insert(new MessageOccurrence(
            chatMessage.Identifier,
            chatMessage.Identifier.GetChannel(),
            chatMessage.Author.Identifier,
            _timeProvider.GetCurrentUtcTime()));
    }

    public int GetMessageCountInChannel(
        string channelId,
        TimeSpan maxTimeAgo)
    {
        DateTimeOffset timeLimit = _timeProvider.GetCurrentUtcTime() - maxTimeAgo;
        return _repository
            .GetAll()
            .Count(o => o.ChannelId == channelId && o.OccurredAt >= timeLimit);
    }
}

