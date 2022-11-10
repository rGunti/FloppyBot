using System.Collections.Immutable;

namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record V1HealthCheckData(
    DateTimeOffset RecordedAt,
    string HostName,
    int Pid,
    float MemoryUsed,
    string Version,
    IImmutableList<MessageInterfaceDescription> MessageInterfaces,
    TimeSpan Uptime);
