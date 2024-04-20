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

public record CommandResponseDto(
    CommandResponseType Type,
    string Content,
    string? AuxiliaryContent = null
)
{
    public static CommandResponseDto FromEntity(CommandResponse entity)
    {
        return entity.Type switch
        {
            ResponseType.Text => new CommandResponseDto(CommandResponseType.Text, entity.Content),
            ResponseType.Sound => ConvertSoundCommand(entity),
            _ => throw new ArgumentOutOfRangeException(nameof(entity.Type), entity.Type, null),
        };
    }

    private static CommandResponseDto ConvertSoundCommand(CommandResponse entity)
    {
        var split = entity.Content.Split(CommandResponse.SOUND_CMD_SPLIT_CHAR);
        var soundFile = split[0];
        var replyMessage = split.Length > 1 ? split[1] : null;

        return new CommandResponseDto(CommandResponseType.Sound, soundFile, replyMessage);
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
