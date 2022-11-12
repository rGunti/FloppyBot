using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Metadata;

namespace FloppyBot.Commands.Executor.Agent.Cmds;

[CommandHost]
[CommandCategory("Diagnostics")]
// ReSharper disable once UnusedType.Global
public class DebugCommands
{
    [Command("debugpriv")]
    // ReSharper disable once UnusedMember.Global
    public static string GetPrivilegeLevel([Author] ChatUser author)
    {
        return $"Your privilege level is: {author.PrivilegeLevel}";
    }
}
