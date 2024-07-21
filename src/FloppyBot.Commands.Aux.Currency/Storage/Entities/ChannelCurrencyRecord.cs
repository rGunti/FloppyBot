using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.Attributes;

namespace FloppyBot.Commands.Aux.Currency.Storage.Entities;

[IndexFields("ChannelUser", nameof(Channel), nameof(User))]
public record ChannelCurrencyRecord(string Id, string Channel, string User, int Balance)
    : IEntity<ChannelCurrencyRecord>
{
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
