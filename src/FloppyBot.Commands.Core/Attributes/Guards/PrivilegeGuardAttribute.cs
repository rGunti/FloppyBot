using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Core.Attributes.Guards;

public class PrivilegeGuardAttribute : GuardAttribute
{
    public PrivilegeGuardAttribute(PrivilegeLevel minLevel)
    {
        MinLevel = minLevel;
    }

    public PrivilegeLevel MinLevel { get; }

    public override string ToString()
    {
        return $"{nameof(PrivilegeGuardAttribute)}({MinLevel})";
    }
}
