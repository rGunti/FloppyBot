namespace FloppyBot.Chat.Discord.Config;

public record DiscordConfiguration(
    string ClientId,
    string ClientSecret,
    string Token,
    long Privileges = 339008);