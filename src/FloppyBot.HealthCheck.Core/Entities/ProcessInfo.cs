namespace FloppyBot.HealthCheck.Core.Entities;

public record ProcessInfo(int Pid, long MemoryConsumed, DateTimeOffset StartedAt);
