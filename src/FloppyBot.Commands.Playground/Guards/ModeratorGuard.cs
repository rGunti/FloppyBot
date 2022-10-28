using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Playground.Guards;

public class ModeratorGuard : PrivilegeLevelGuard
{
    public ModeratorGuard() : base(PrivilegeLevel.Moderator)
    {
    }
}
