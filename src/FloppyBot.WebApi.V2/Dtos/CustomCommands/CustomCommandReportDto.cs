using FloppyBot.Base.EquatableCollections;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Custom.Storage.Entities;

namespace FloppyBot.WebApi.V2.Dtos.CustomCommands;

public record CustomCommandDto(
    string Id,
    string Name,
    string[] Aliases,
    CommandResponseDto[] Responses,
    CommandLimitationDto Limitations,
    CommandResponseMode ResponseMode,
    CounterValueDto? Counter,
    bool AllowCounterOperations
)
{
    public static CustomCommandDto FromEntity(CustomCommandDescription entity, int? counter)
    {
        return new CustomCommandDto(
            entity.Id,
            entity.Name,
            entity.Aliases.OrderBy(s => s).ToArray(),
            entity.Responses.Select(CommandResponseDto.FromEntity).ToArray(),
            CommandLimitationDto.FromEntity(entity.Limitations),
            ToEntityMode(entity.ResponseMode),
            counter.HasValue ? new CounterValueDto(counter.Value) : null,
            entity.AllowCounterOperations
        );
    }

    public CustomCommandDescription ToEntity()
    {
        return new CustomCommandDescription
        {
            Id = Id,
            Name = Name,
            Aliases = Aliases.ToImmutableHashSetWithValueSemantics(),
            Responses = Responses.Select(r => r.ToEntity()).ToImmutableListWithValueSemantics(),
            Limitations = Limitations.ToEntity(),
            ResponseMode = ResponseMode.ToEntity(),
            AllowCounterOperations = AllowCounterOperations,
        };
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

    public CommandResponse ToEntity()
    {
        return Type switch
        {
            CommandResponseType.Text => new CommandResponse(ResponseType.Text, Content),
            CommandResponseType.Sound => ConvertSoundCommand(this),
            _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, null),
        };
    }

    private static CommandResponseDto ConvertSoundCommand(CommandResponse entity)
    {
        var split = entity.Content.Split(CommandResponse.SOUND_CMD_SPLIT_CHAR);
        var soundFile = split[0];
        var replyMessage = split.Length > 1 ? split[1] : null;

        return new CommandResponseDto(CommandResponseType.Sound, soundFile, replyMessage);
    }

    private static CommandResponse ConvertSoundCommand(CommandResponseDto dto)
    {
        var content = dto.Content;
        if (dto.AuxiliaryContent is not null)
        {
            content += CommandResponse.SOUND_CMD_SPLIT_CHAR + dto.AuxiliaryContent;
        }

        return new CommandResponse(ResponseType.Sound, content);
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
            entity
                .Cooldown.Select(CooldownDescriptionDto.FromEntity)
                .OrderBy(c => c.Level)
                .ToArray(),
            entity.LimitedToUsers.OrderBy(s => s).ToArray()
        );
    }

    public CommandLimitation ToEntity()
    {
        return new CommandLimitation
        {
            MinLevel = Enum.Parse<PrivilegeLevel>(MinLevel),
            Cooldown = Cooldown.Select(c => c.ToEntity()).ToImmutableHashSetWithValueSemantics(),
            LimitedToUsers = LimitedToUsers.ToImmutableHashSetWithValueSemantics(),
        };
    }
}

public record CooldownDescriptionDto(PrivilegeLevel Level, int Milliseconds)
{
    public static CooldownDescriptionDto FromEntity(CooldownDescription entity)
    {
        return new CooldownDescriptionDto(entity.Level, entity.Milliseconds);
    }

    public CooldownDescription ToEntity()
    {
        return new CooldownDescription(Level, Milliseconds);
    }
}

public enum CommandResponseMode
{
    First,
    PickOneRandom,
    All,
}

public record CounterValueDto(int Value);

public static class CommandResponseModeExtensions
{
    public static Commands.Custom.Storage.Entities.CommandResponseMode ToEntity(
        this CommandResponseMode dtoMode
    )
    {
        return dtoMode switch
        {
            CommandResponseMode.First => Commands.Custom.Storage.Entities.CommandResponseMode.First,
            CommandResponseMode.PickOneRandom
                => Commands.Custom.Storage.Entities.CommandResponseMode.PickOneRandom,
            CommandResponseMode.All => Commands.Custom.Storage.Entities.CommandResponseMode.All,
            _ => throw new ArgumentOutOfRangeException(nameof(dtoMode), dtoMode, null),
        };
    }
}

public enum CommandResponseType
{
    Text,
    Sound,
}
