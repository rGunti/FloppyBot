using System.Collections.Immutable;
using FloppyBot.Base.Clock;
using FloppyBot.Base.Storage;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;

namespace FloppyBot.Commands.Aux.Quotes.Storage;

/*
 * TODOs for later:
 * - Setup a clean job to remove unconfirmed channel mappings & expired join keys
 */

public class QuoteChannelMappingService : IQuoteChannelMappingService
{
    private static readonly TimeSpan MaxCodeAge = 5.Minutes();
    private readonly IRepository<QuoteChannelMappingJoinKeys> _joinKeyRepository;

    private readonly IRepository<QuoteChannelMapping> _repository;
    private readonly ITimeProvider _timeProvider;

    public QuoteChannelMappingService(
        IRepositoryFactory repositoryFactory,
        ITimeProvider timeProvider
    )
    {
        _timeProvider = timeProvider;
        _repository = repositoryFactory.GetRepository<QuoteChannelMapping>();
        _joinKeyRepository = repositoryFactory.GetRepository<QuoteChannelMappingJoinKeys>();
    }

    public string? GetQuoteChannelMapping(string channelId, bool createIfMissing = false)
    {
        var mapping = _repository
            .GetAll()
            .FirstOrDefault(m => m.ChannelId == channelId && m.Confirmed);

        if (mapping == null && createIfMissing)
        {
            mapping = CreateNewMapping(channelId);
        }

        return mapping?.MappingId;
    }

    public string? StartJoinProcess(string mappingId, string newChannelId)
    {
        var mappings = GetQuoteChannelMappings(mappingId).ToImmutableList();

        if (!mappings.Any() || mappings.Any(c => c.ChannelId == newChannelId))
        {
            return null;
        }

        StageNewChannelToMapping(mappingId, newChannelId);
        return CreateJoinKey(mappingId, newChannelId);
    }

    public bool ConfirmJoinProcess(string mappingId, string newChannelId, string joinCode)
    {
        var mappings = GetQuoteChannelMappings(mappingId).Where(m => !m.Confirmed);
        if (!mappings.Any())
        {
            return false;
        }

        var key = _joinKeyRepository.GetById(joinCode);
        if (key == null)
        {
            return false;
        }

        if (newChannelId != key.ChannelId || mappingId != key.MappingId)
        {
            return false;
        }

        if (key.ExpiresAt < _timeProvider.GetCurrentUtcTime())
        {
            DeleteUnconfirmedChannelMappings(mappingId);
            DeleteJoinKey(key);
            return false;
        }

        ConfirmNewChannelToMapping(mappingId, newChannelId);
        DeleteJoinKey(key);
        return true;
    }

    private IEnumerable<QuoteChannelMapping> GetQuoteChannelMappings(string mappingId)
    {
        return _repository.GetAll().Where(m => m.MappingId == mappingId);
    }

    private string CreateJoinKey(string mappingId, string channelId)
    {
        return _joinKeyRepository
            .Insert(
                new QuoteChannelMappingJoinKeys(
                    Guid.NewGuid().ToString(),
                    mappingId,
                    channelId,
                    _timeProvider.GetCurrentUtcTime().Add(MaxCodeAge).UtcDateTime
                )
            )
            .Id;
    }

    private QuoteChannelMapping CreateNewMapping(string channelId)
    {
        var mapping = new QuoteChannelMapping(null!, Guid.NewGuid().ToString(), channelId, true);
        return _repository.Insert(mapping);
    }

    private void StageNewChannelToMapping(string mappingId, string channelId)
    {
        _repository.Insert(new QuoteChannelMapping(null!, mappingId, channelId, false));
    }

    private void ConfirmNewChannelToMapping(string mappingId, string channelId)
    {
        var mapping = _repository
            .GetAll()
            .First(m => m.MappingId == mappingId && m.ChannelId == channelId);
        _repository.Update(mapping with { Confirmed = true });
    }

    private void DeleteJoinKey(QuoteChannelMappingJoinKeys key)
    {
        _joinKeyRepository.Delete(key);
    }

    private void DeleteUnconfirmedChannelMappings(string mappingId)
    {
        _repository.Delete(
            _repository.GetAll().Where(m => m.MappingId == mappingId && !m.Confirmed)
        );
    }
}
