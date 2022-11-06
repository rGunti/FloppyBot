using System.Collections.Immutable;

namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record HealthCheckData(
    DateTimeOffset RecordedAt,
    string HostName,
    int Pid,
    float MemoryUsed,
    string Version,
    IImmutableList<MessageInterfaceDescription> MessageInterfaceDescription,
    TimeSpan Uptime);
