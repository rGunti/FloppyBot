using FloppyBot.Chat.Discord.Config;

namespace FloppyBot.IntTest.Config;

public record IntegrationTestingConfiguration(
    DiscordConfiguration DiscordBot1,
    DiscordConfiguration DiscordBot2,
    string DiscordServerId,
    string DiscordChannelId
)
{
    [Obsolete("This constructor is only present for configuration purposes and should not be used")]
    public IntegrationTestingConfiguration()
        : this(null!, null!, string.Empty, string.Empty) { }
}
