using FloppyBot.Base.Auditing.Abstraction.Entities;
using FloppyBot.Base.Storage;

namespace FloppyBot.Base.Auditing.Storage;

internal record InternalAuditRecord(
    string Id,
    DateTimeOffset Timestamp,
    string UserIdentifier,
    string ChannelIdentifier,
    string ObjectType,
    string ObjectIdentifier,
    string Action,
    string? AdditionalData
) : IEntity<InternalAuditRecord>
{
    public InternalAuditRecord WithId(string newId)
    {
        return this with { Id = newId };
    }

    public AuditRecord ToAuditRecord()
    {
        return new AuditRecord(
            Id,
            Timestamp,
            UserIdentifier,
            ChannelIdentifier,
            ObjectType,
            ObjectIdentifier,
            Action,
            AdditionalData
        );
    }

    public static InternalAuditRecord FromAuditRecord(AuditRecord auditRecord)
    {
        return new InternalAuditRecord(
            auditRecord.Id,
            auditRecord.Timestamp,
            auditRecord.UserIdentifier,
            auditRecord.ChannelIdentifier,
            auditRecord.ObjectType,
            auditRecord.ObjectIdentifier,
            auditRecord.Action,
            auditRecord.AdditionalData
        );
    }
}
