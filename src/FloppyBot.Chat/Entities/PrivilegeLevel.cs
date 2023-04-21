namespace FloppyBot.Chat.Entities;

public enum PrivilegeLevel
{
    Unknown = 0,
    Viewer = 1,
    Moderator = 2,
    Administrator = 4,
    Superuser = byte.MaxValue,
}
