using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Base.Auditing.Abstraction.Entities;
using FloppyBot.Base.Clock;
using FloppyBot.Base.Storage;

namespace FloppyBot.Base.Auditing.Storage;

/// <summary>
/// Implementation of <see cref="IAuditor"/> that stores audit records in a repository.
/// </summary>
public class StorageAuditor : IAuditor
{
    private readonly IRepository<InternalAuditRecord> _repository;
    private readonly ITimeProvider _timeProvider;

    public StorageAuditor(IRepositoryFactory repositoryFactory, ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _repository = repositoryFactory.GetRepository<InternalAuditRecord>("AuditRecord");
    }

    /// <inheritdoc />
    public void Record(AuditRecord auditRecord)
    {
        _repository.Insert(
            InternalAuditRecord.FromAuditRecord(auditRecord) with
            {
                Timestamp = _timeProvider.GetCurrentUtcTime(),
            }
        );
    }

    /// <inheritdoc />
    public IEnumerable<AuditRecord> GetAuditRecords(params string[] channels)
    {
        return _repository
            .GetAll()
            .Where(c => channels.Contains(c.ChannelIdentifier))
            .ToList()
            .Select(i => i.ToAuditRecord())
            .OrderByDescending(i => i.Timestamp);
    }
}
