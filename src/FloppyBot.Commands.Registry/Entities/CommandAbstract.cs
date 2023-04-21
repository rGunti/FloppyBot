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
    bool NoParameters,
    bool Hidden,
    CommandParameterAbstract[] Parameters,
    Dictionary<string, string> AllMetadata
);

/// <remarks>Impure!</remarks>
public record CommandParameterAbstract(
    int Order,
    string Name,
    CommandParameterAbstractType Type,
    bool Required,
    string? Description,
    string[]? PossibleValues
);

public enum CommandParameterAbstractType
{
    String,
    Number,
    Enum
}
