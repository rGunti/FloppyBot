using FloppyBot.Base.Auditing.Abstraction.Entities;

namespace FloppyBot.Base.Auditing.Abstraction;

/// <summary>
/// A service that records events for auditing purposes
/// </summary>
public interface IAuditor
{
    /// <summary>
    /// Records a new event
    /// </summary>
    /// <param name="auditRecord"></param>
    void Record(AuditRecord auditRecord);

    /// <summary>
    /// Get a list of audit records for the specified channels
    /// </summary>
    /// <param name="channel">Channel to query from</param>
    /// <param name="channels">Additional channels to query from</param>
    /// <returns></returns>
    IEnumerable<AuditRecord> GetAuditRecords(string channel, params string[] channels);
}
