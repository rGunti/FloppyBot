using System.Collections.Immutable;
using AutoMapper;
using FloppyBot.Base.EquatableCollections;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;
using FloppyBot.Commands.Aux.Twitch.Storage.Entities;
using FloppyBot.Commands.Custom.Communication.Entities;
using FloppyBot.Commands.Custom.Execution;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.HealthCheck.Core.Entities;
using FloppyBot.WebApi.Auth.Dtos;
using FloppyBot.WebApi.V1Compatibility.Dtos;

namespace FloppyBot.WebApi.V1Compatibility.Mapping;

public class V1CompatibilityProfile : Profile
{
    public V1CompatibilityProfile()
    {
        MapQuote();
        MapUser();
        MapShoutoutMessage();
        MapHealthCheckData();
        MapCustomCommands();
        MapSoundCommand();
        MapFileStorage();
        MapSoundCommandInvocation();
    }

    private void MapQuote()
    {
        CreateMap<Quote, QuoteDto>()
            .ConstructUsing(q => new QuoteDto(
                q.Id,
                q.ChannelMappingId,
                q.QuoteId,
                q.QuoteText,
                q.QuoteContext,
                q.CreatedAt,
                q.CreatedBy))
            .ForAllMembers(o => o.Ignore());
        CreateMap<QuoteDto, Quote>()
            .ConstructUsing(dto => new Quote(
                dto.Id,
                dto.Channel,
                dto.QuoteId,
                dto.QuoteText,
                dto.QuoteContext ?? string.Empty,
                dto.CreatedAt,
                dto.CreatedBy))
            .ForAllMembers(o => o.Ignore());
    }

    private void MapUser()
    {
        CreateMap<User, UserDto>()
            .ConstructUsing(u => new UserDto(
                u.Id,
                u.OwnerOf.ToImmutableListWithValueSemantics(),
                u.ChannelAliases.ToImmutableDictionary()))
            .ForAllMembers(o => o.Ignore());
    }

    private void MapShoutoutMessage()
    {
        CreateMap<ShoutoutMessageConfig, ShoutoutMessageSetting>()
            .ConstructUsing(c => new ShoutoutMessageSetting(
                c.Id,
                c.Message));
        CreateMap<ShoutoutMessageSetting, ShoutoutMessageConfig>()
            .ConstructUsing(c => new ShoutoutMessageConfig(
                c.Id,
                c.Message));
    }

    private void MapHealthCheckData()
    {
        CreateMap<HealthCheckData, V1HealthCheckData>()
            .ConstructUsing(c => new V1HealthCheckData(
                c.RecordedAt,
                c.HostName,
                c.Process.Pid,
                c.Process.MemoryConsumed,
                c.App.Version,
                new[]
                {
                    new MessageInterfaceDescription(
                        c.App.InstanceName,
                        $"{c.App.InstanceName} ({c.App.Service})",
                        c.App.Service,
                        true,
                        string.Empty,
                        null,
                        null,
                        null)
                }.ToImmutableListWithValueSemantics(),
                DateTimeOffset.UtcNow - c.Process.StartedAt));
    }

    private void MapCustomCommands()
    {
        CreateMap<CustomCommandDescription, CustomCommand>()
            .ConstructUsing(c => new CustomCommand(
                c.Id,
                c.Owners.First(),
                c.Name,
                c.Responses.Count == 1 ? c.Responses.Select(r => r.Content).First() : string.Empty,
                c.Responses.Count == 1
                    ? ImmutableListWithValueSemantics<string>.Empty
                    : c.Responses
                        .Where(r => r.Type == ResponseType.Text)
                        .Select(r => r.Content)
                        .ToImmutableList(),
                c.Limitations.MinLevel >= PrivilegeLevel.Moderator,
                Array.Empty<string>().ToImmutableList(),
                c.Limitations.Cooldown
                    .OrderBy(cd => cd.Level)
                    .Select(cd => cd.Milliseconds)
                    .FirstOrDefault()));
        CreateMap<CustomCommand, CustomCommandDescription>()
            .ConstructUsing(c => new CustomCommandDescription
            {
                Name = c.Command,
                Aliases = ImmutableSetWithValueSemantics<string>.Empty,
                Owners = new[] { c.Channel }.ToImmutableHashSetWithValueSemantics(),
                Limitations = new CommandLimitation
                {
                    MinLevel = c.LimitedToMod ? PrivilegeLevel.Moderator : PrivilegeLevel.Viewer,
                    Cooldown = new[]
                    {
                        new CooldownDescription(PrivilegeLevel.Unknown, c.Timeout)
                    }.ToImmutableHashSetWithValueSemantics()
                },
                ResponseMode = CommandResponseMode.PickOneRandom,
                Responses = (c.ResponseVariants ?? Enumerable.Empty<string>()).Any()
                    ? c.ResponseVariants!
                        .Select(r => new CommandResponse(ResponseType.Text, r))
                        .ToImmutableListWithValueSemantics()
                    : new[]
                    {
                        new CommandResponse(ResponseType.Text, c.Response)
                    }.ToImmutableListWithValueSemantics(),
                Id = null!,
            });
    }

    private void MapSoundCommand()
    {
        CreateMap<CustomCommandDescription, SoundCommand>()
            .ConstructUsing(c => new SoundCommand(
                c.Id,
                c.Name,
                c.Owners.First(),
                c.Limitations.MinLevel >= PrivilegeLevel.Moderator,
                Array.Empty<string>().ToImmutableList(),
                false,
                c.Limitations.Cooldown
                    .OrderBy(cd => cd.Level)
                    .Select(cd => cd.Milliseconds)
                    .FirstOrDefault(),
                c.Responses
                    .Where(r => r.Type == ResponseType.Text)
                    .Select(r => r.Content.Substring(r.Content.IndexOf(CustomCommandExecutor.SOUND_CMD_SPLIT_CHAR))
                        .Trim())
                    .FirstOrDefault()!,
                c.Responses
                    .Where(r => r.Type == ResponseType.Sound)
                    .Select(r => r.Content.Substring(0, r.Content.IndexOf(CustomCommandExecutor.SOUND_CMD_SPLIT_CHAR)))
                    .ToImmutableListWithValueSemantics()));

        CreateMap<SoundCommand, CustomCommandDescription>()
            .ConstructUsing(c => new CustomCommandDescription
            {
                Name = c.CommandName,
                Owners = new[] { c.ChannelId }.ToImmutableHashSet(),
                Limitations = new CommandLimitation
                {
                    MinLevel = c.LimitedToMod ? PrivilegeLevel.Moderator : PrivilegeLevel.Viewer,
                    Cooldown = new[]
                    {
                        new CooldownDescription(PrivilegeLevel.Unknown, c.Cooldown)
                    }.ToImmutableHashSetWithValueSemantics()
                },
                ResponseMode = CommandResponseMode.PickOneRandom,
                Responses = new[]
                {
                    new CommandResponse(
                        ResponseType.Sound,
                        $"{c.SoundFiles[0]}{CustomCommandExecutor.SOUND_CMD_SPLIT_CHAR}{c.Response}")
                }.ToImmutableList(),
                Id = null!,
            });
    }

    private void MapFileStorage()
    {
        CreateMap<FloppyBot.FileStorage.Entities.FileHeader, FileHeader>()
            .ConstructUsing(f => new FileHeader(
                f.Id,
                f.Owner,
                f.FileName,
                (long)f.FileSize,
                f.MimeType));
    }

    private void MapSoundCommandInvocation()
    {
        CreateMap<SoundCommandInvocation, InvokeSoundCommandEvent>()
            .ConstructUsing(i => new InvokeSoundCommandEvent(
                i.InvokedBy,
                i.InvokedFrom,
                i.CommandName,
                i.InvokedAt));
    }

    public static bool IsConvertableForTextCommand(CustomCommandDescription commandDescription)
    {
        return commandDescription.Aliases.Count == 0
               && commandDescription.Responses.All(r => r.Type == ResponseType.Text);
    }

    public static bool IsConvertableForSoundCommand(CustomCommandDescription commandDescription)
    {
        return commandDescription.Aliases.Count == 0
               && commandDescription.Responses.All(r => r.Type == ResponseType.Sound);
    }
}
