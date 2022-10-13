using FloppyBot.Chat.Entities;

namespace FloppyBot.Chat.Console.Agent;

internal record ConsoleAgentUserConfiguration(
    string Username,
    PrivilegeLevel PrivilegeLevel)
{
    // ReSharper disable once UnusedMember.Global
    public ConsoleAgentUserConfiguration()
        : this("NotConfigured", PrivilegeLevel.Unknown)
    {
    }
}
