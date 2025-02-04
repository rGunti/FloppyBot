using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.Attributes;

namespace FloppyBot.Commands.Aux.Currency.Storage.Entities;

[IndexFields("ChannelUser", nameof(Channel), nameof(User))]
public record ChannelCurrencyRecord(string Id, string Channel, string User, int Balance)
    : IEntity<ChannelCurrencyRecord>
{
    public static ChannelCurrencyRecord ForUserInChannel(
        string channelId,
        string userId,
        int balance = 0
    )
    {
        return new(null!, channelId.ToLowerInvariant(), userId.ToLowerInvariant(), balance);
    }

    public ChannelCurrencyRecord WithId(string newId)
    {
        return this with { Id = newId };
    }
}

public record ChannelCurrencySettings(
    string Id,
    bool Enabled,
    string CurrencyName,
    int StartingBalance
) : IEntity<ChannelCurrencySettings>
{
    private static readonly ChannelCurrencySettings Default = new(null!, true, "Floppies", 100);

    public static ChannelCurrencySettings GetDefault(string channelId)
    {
        return Default with { Id = channelId };
    }

    public ChannelCurrencySettings WithId(string newId)
    {
        return this with { Id = newId };
    }
}

public record ChannelUserState(string Id, UserState State, DateTimeOffset LastStateChange)
    : IEntity<ChannelUserState>
{
    private static readonly ChannelUserState Default = new(
        null!,
        UserState.Offline,
        DateTimeOffset.MaxValue
    );

    public static ChannelUserState ForUserInChannel(string userId, string channelId)
    {
        return Default with { Id = GetId(channelId, userId) };
    }

    public static string GetId(string userId, string channelId)
    {
        return $"{userId}@{channelId}";
    }

    public ChannelUserState WithId(string newId)
    {
        return this with { Id = newId };
    }
}

public enum UserState
{
    Offline,
    Online,
}
