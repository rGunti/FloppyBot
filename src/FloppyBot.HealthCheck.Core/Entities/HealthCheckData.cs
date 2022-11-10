namespace FloppyBot.HealthCheck.Core.Entities;

public record HealthCheckData(
    DateTimeOffset RecordedAt,
    string InstanceId,
    string HostName,
    AppInfo App,
    ProcessInfo Process);
