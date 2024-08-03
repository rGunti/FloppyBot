using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Aux.Currency.Storage.Entities;

namespace FloppyBot.Commands.Aux.Currency;

public static class ChannelCurrencyAuditing
{
    public static void CurrencyUpdated(
        this IAuditor auditor,
        ChatUser user,
        ChannelIdentifier channel,
        ChannelCurrencyRecord record
    )
    {
        auditor.Record(
            user.Identifier,
            channel,
            @record,
            r => r.User,
            CommonActions.Updated,
            record.Balance.ToString()
        );
    }

    public static void CurrencyDeleted(
        this IAuditor auditor,
        ChatUser user,
        ChannelIdentifier channel
    )
    {
        auditor.Record(
            user.Identifier,
            channel,
            nameof(ChannelCurrencyRecord),
            user.Identifier.Channel,
            CommonActions.Deleted
        );
    }
}
