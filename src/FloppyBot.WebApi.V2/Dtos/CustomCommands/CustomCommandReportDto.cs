using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Custom.Storage.Entities;

namespace FloppyBot.WebApi.V2.Dtos.CustomCommands;

public record CustomCommandDto(
    string Id,
    string Name,
    string[] Aliases,
    CommandResponseDto[] Responses,
    CommandLimitation Limitations,
    CommandResponseMode ResponseMode
)
{
    public static CustomCommandDto FromEntity(CustomCommandDescription entity)
    {
        return new CustomCommandDto(
            entity.Id,
            entity.Name,
            entity.Aliases.ToArray(),
            entity.Responses.Select(CommandResponseDto.FromEntity).ToArray(),
            entity.Limitations,
            ToEntityMode(entity.ResponseMode)
        );
    }

    private static CommandResponseMode ToEntityMode(
        Commands.Custom.Storage.Entities.CommandResponseMode dtoMode
    )
    {
        return dtoMode switch
        {
            Commands.Custom.Storage.Entities.CommandResponseMode.First => CommandResponseMode.First,
            Commands.Custom.Storage.Entities.CommandResponseMode.PickOneRandom
                => CommandResponseMode.PickOneRandom,
            Commands.Custom.Storage.Entities.CommandResponseMode.All => CommandResponseMode.All,
            _ => throw new ArgumentOutOfRangeException(nameof(dtoMode), dtoMode, null),
        };
    }
}

public record CommandResponseDto(CommandResponseType Type, string Content)
{
    public static CommandResponseDto FromEntity(CommandResponse entity)
    {
        return new CommandResponseDto(ToDtoType(entity.Type), entity.Content);
    }

    private static CommandResponseType ToDtoType(ResponseType entityType)
    {
        return entityType switch
        {
            ResponseType.Text => CommandResponseType.Text,
            ResponseType.Sound => CommandResponseType.Sound,
            _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null),
        };
    }
}

public record CommandLimitationDto(
    string MinLevel,
    CooldownDescriptionDto[] Cooldown,
    string[] LimitedToUsers
)
{
    public static CommandLimitationDto FromEntity(CommandLimitation entity)
    {
        return new CommandLimitationDto(
            entity.MinLevel.ToString(),
            entity.Cooldown.Select(CooldownDescriptionDto.FromEntity).ToArray(),
            entity.LimitedToUsers.ToArray()
        );
    }
}

public record CooldownDescriptionDto(PrivilegeLevel Level, int Milliseconds)
{
    public static CooldownDescriptionDto FromEntity(CooldownDescription entity)
    {
        return new CooldownDescriptionDto(entity.Level, entity.Milliseconds);
    }
}

public enum CommandResponseMode
{
    First,
    PickOneRandom,
    All,
}

public enum CommandResponseType
{
    Text,
    Sound,
}
