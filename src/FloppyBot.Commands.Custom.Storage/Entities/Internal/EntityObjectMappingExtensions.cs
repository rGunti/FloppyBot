using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using FloppyBot.Commands.Custom.Storage.Entities;

[assembly: InternalsVisibleTo("FloppyBot.Commands.Custom.Tests")]

namespace FloppyBot.Commands.Custom.Storage.Entities.Internal;

internal static class EntityObjectMappingExtensions
{
    public static CooldownDescriptionEo ToEo(this CooldownDescription dto) =>
        new() { Level = dto.Level, Milliseconds = dto.Milliseconds };

    public static CooldownDescription ToDto(this CooldownDescriptionEo eo) =>
        new(eo.Level, eo.Milliseconds);

    public static CommandResponseEo ToEo(this CommandResponse dto) =>
        new()
        {
            Type = $"{dto.Type}",
            Content = dto.Content,
            SendAsReply = dto.SendAsReply,
        };

    public static CommandResponse ToDto(this CommandResponseEo eo) =>
        new(Enum.Parse<ResponseType>(eo.Type), eo.Content, eo.SendAsReply);

    public static CommandLimitationEo ToEo(this CommandLimitation dto) =>
        new()
        {
            MinLevel = dto.MinLevel,
            Cooldown = dto.Cooldown.Select(c => c.ToEo()).ToArray(),
            LimitedToUsers = dto.LimitedToUsers.OrderBy(u => u).ToArray(),
        };

    public static CommandLimitation ToDto(this CommandLimitationEo eo) =>
        new()
        {
            MinLevel = eo.MinLevel,
            Cooldown = eo.Cooldown.Select(c => c.ToDto()).ToImmutableHashSet(),
            LimitedToUsers = (eo.LimitedToUsers ?? Array.Empty<string>()).ToImmutableHashSet(),
        };

    public static CustomCommandDescriptionEo ToEo(this CustomCommandDescription dto) =>
        new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Aliases = dto.Aliases.OrderBy(a => a).ToArray(),
            Owners = dto.Owners.OrderBy(o => o).ToArray(),
            Responses = dto.Responses.Select(r => r.ToEo()).ToArray(),
            Limitations = dto.Limitations.ToEo(),
            ResponseMode = dto.ResponseMode,
            AllowCounterOperations = dto.AllowCounterOperations,
        };

    public static CustomCommandDescription ToDto(this CustomCommandDescriptionEo eo) =>
        new()
        {
            Id = eo.Id,
            Name = eo.Name,
            Aliases = eo.Aliases.ToImmutableSortedSet(),
            Owners = eo.Owners.ToImmutableSortedSet(),
            Responses = eo.Responses.Select(r => r.ToDto()).ToImmutableList(),
            Limitations = eo.Limitations.ToDto(),
            ResponseMode = eo.ResponseMode,
            AllowCounterOperations = eo.AllowCounterOperations,
        };
}
