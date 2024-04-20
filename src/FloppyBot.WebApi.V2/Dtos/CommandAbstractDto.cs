using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Registry.Entities;

namespace FloppyBot.WebApi.V2.Dtos;

public record CommandAbstractDto(
    string Name,
    string[] Aliases,
    string? Description,
    PrivilegeLevel? MinPrivilegeLevel,
    string[] AvailableOnInterfaces,
    string[]? Syntax,
    bool NoParameters,
    bool Hidden,
    CommandParameterAbstractDto[] Parameters,
    Dictionary<string, string> AllMetadata
)
{
    public static CommandAbstractDto FromEntity(CommandAbstract entity)
    {
        return new CommandAbstractDto(
            entity.Name,
            entity.Aliases,
            entity.Description,
            entity.MinPrivilegeLevel,
            entity.AvailableOnInterfaces,
            entity.Syntax,
            entity.NoParameters,
            entity.Hidden,
            entity.Parameters.Select(CommandParameterAbstractDto.FromEntity).ToArray(),
            entity.AllMetadata
        );
    }
}
