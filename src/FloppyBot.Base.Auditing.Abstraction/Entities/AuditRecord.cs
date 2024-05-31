namespace FloppyBot.Base.Auditing.Abstraction.Entities;

public record AuditRecord(
    string Id,
    DateTimeOffset Timestamp,
    string UserIdentifier,
    string ChannelIdentifier,
    string ObjectType,
    string ObjectIdentifier,
    string Action,
    string? AdditionalData
);
