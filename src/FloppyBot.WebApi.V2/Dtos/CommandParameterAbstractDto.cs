using FloppyBot.Commands.Registry.Entities;

namespace FloppyBot.WebApi.V2.Dtos;

public record CommandParameterAbstractDto(
    int Order,
    string Name,
    CommandParameterAbstractType Type,
    bool Required,
    string? Description,
    string[]? PossibleValues
)
{
    public static CommandParameterAbstractDto FromEntity(CommandParameterAbstract entity)
    {
        return new CommandParameterAbstractDto(
            entity.Order,
            entity.Name,
            entity.Type,
            entity.Required,
            entity.Description,
            entity.PossibleValues
        );
    }
}
