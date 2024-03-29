﻿namespace FloppyBot.Chat.Discord.Config;

public record DiscordConfiguration(
    string ClientId,
    string ClientSecret,
    string Token,
    long Privileges,
    string CommandPrefix,
    bool ReadBotMessages = false
)
{
    [Obsolete("This constructor is only present for configuration purposes and should not be used")]
    // ReSharper disable once UnusedMember.Global
    public DiscordConfiguration()
        : this(string.Empty, string.Empty, string.Empty, 339008, string.Empty) { }
}
