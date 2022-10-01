using FloppyBot.Chat.Config;
using FloppyBot.Chat.Discord;
using FloppyBot.Chat.Discord.Config;
using FloppyBot.Chat.Mock;
using FloppyBot.Chat.Twitch;
using FloppyBot.Chat.Twitch.Config;

namespace FloppyBot.Chat.Agent;

public static class InterfaceRegistration
{
    private static readonly Dictionary<string, Func<IServiceCollection, IServiceCollection>> InterfaceTypes = new()
    {
        { MockChatInterface.IF_NAME, s => s.AddMockChatInterface() },
        { TwitchChatInterface.IF_NAME, s => s.AddTwitchChatInterface() },
        { DiscordChatInterface.IF_NAME, s => s.AddDiscordChatInterface() }
    };

    public static IServiceCollection RegisterChatInterface(this IServiceCollection services, string interfaceType)
    {
        return InterfaceTypes[interfaceType].Invoke(services);
    }
}
