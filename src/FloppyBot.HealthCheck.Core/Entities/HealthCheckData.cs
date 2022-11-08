namespace FloppyBot.HealthCheck.Core.Entities;

public record HealthCheckData(
    DateTimeOffset RecordedAt,
    string HostName,
    AppInfo App,
    ProcessInfo Process);
