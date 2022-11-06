using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Registry.Entities;

/// <remarks>Impure!</remarks>
public record CommandAbstract(
    string HostProcess,
    DateTimeOffset LastReported,
    string Name,
    string[] Aliases,
    string? Description,
    PrivilegeLevel? MinPrivilegeLevel,
    string[] AvailableOnInterfaces,
    string[]? Syntax,
    Dictionary<string, string> AllMetadata);
