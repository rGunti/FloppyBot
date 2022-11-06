using System.Collections.Immutable;
using FloppyBot.Chat.Entities;

namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record CommandInfo(
    string Name,
    string Description,
    IImmutableList<string> Aliases,
    IImmutableList<string> RestrictedToInterfaces,
    PrivilegeLevel RequiredPrivilegeLevel,
    bool CustomCommand,
    CooldownInfo Cooldown,
    IImmutableList<CommandSyntax> Syntax,
    bool Obsolete);
