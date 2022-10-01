using Discord;
using Discord.WebSocket;
using FloppyBot.Base.Testing;
using FloppyBot.Chat.Discord.Config;

namespace FloppyBot.Chat.Discord.Tests;

[TestClass]
public class DiscordChatInterfaceTests
{
    private DiscordChatInterface CreateInterface()
    {
        return new DiscordChatInterface(
            LoggingUtils.GetLogger<DiscordChatInterface>(),
            LoggingUtils.GetLogger<DiscordSocketClient>(),
            new DiscordConfiguration(
                "aClientId",
                "aClientSecret",
                "aToken"),
            Moq.Mock.Of<BaseSocketClient>());
    }
    
    [TestMethod]
    public void TestMethod1()
    {
    }
}