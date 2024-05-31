using FloppyBot.Base.Auditing.Abstraction.Entities;

namespace FloppyBot.Base.Auditing.Abstraction.Impl;

/// <summary>
/// A no-op auditor that does nothing, useful for testing
/// </summary>
public class NoopAuditor : IAuditor
{
    /// <inheritdoc />
    public void Record(AuditRecord auditRecord) { }

    /// <inheritdoc />
    public IEnumerable<AuditRecord> GetAuditRecords(string channel, params string[] channels)
    {
        return [];
    }
}
