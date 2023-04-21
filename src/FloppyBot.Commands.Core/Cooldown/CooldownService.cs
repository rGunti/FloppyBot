using FloppyBot.Base.Clock;
using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Core.Cooldown;

public class CooldownService : ICooldownService
{
    private readonly IRepository<CooldownRecord> _repository;
    private readonly ITimeProvider _timeProvider;

    public CooldownService(IRepositoryFactory repositoryFactory, ITimeProvider timeProvider)
    {
        _repository = repositoryFactory.GetRepository<CooldownRecord>();
        _timeProvider = timeProvider;
    }

    public DateTimeOffset GetLastExecution(string channelId, string userId, string commandId)
    {
        return _repository.GetById(GetRecordId(channelId, userId, commandId))?.ExecutedAt
            ?? DateTimeOffset.MinValue;
    }

    public void StoreExecution(string channelId, string userId, string commandId)
    {
        string id = GetRecordId(channelId, userId, commandId);
        CooldownRecord? stored = _repository.GetById(id);
        if (stored == null)
        {
            _repository.Insert(new CooldownRecord(id, _timeProvider.GetCurrentUtcTime()));
        }
        else
        {
            _repository.Update(stored with { ExecutedAt = _timeProvider.GetCurrentUtcTime() });
        }
    }

    private static string GetRecordId(string channelId, string userId, string commandId)
    {
        return string.Join('|', channelId, userId, commandId);
    }
}
