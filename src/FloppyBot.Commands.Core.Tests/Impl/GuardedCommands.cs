using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Guards;

namespace FloppyBot.Commands.Core.Tests.Impl;

[CommandHost]
public class GuardedCommands
{
    [Command("adminonly")]
    [PrivilegeGuard(PrivilegeLevel.Administrator)]
    public string AdminOnlyCommand()
    {
        return "Only for Admins!";
    }
}
