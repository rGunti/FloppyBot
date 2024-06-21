using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;

namespace FloppyBot.Commands.Aux.Quotes.Storage;

public static class QuoteAuditing
{
    public static void QuoteCreated(
        this IAuditor auditor,
        ChatUser user,
        ChannelIdentifier channel,
        Quote? quote
    )
    {
        if (quote is null)
        {
            return;
        }

        auditor.Record(
            user.Identifier,
            channel,
            quote,
            q => q.QuoteId.ToString(),
            CommonActions.Created,
            quote.ToString()
        );
    }

    public static void QuoteUpdated(
        this IAuditor auditor,
        ChatUser user,
        ChannelIdentifier channel,
        Quote? quote
    )
    {
        if (quote is null)
        {
            return;
        }

        auditor.Record(
            user.Identifier,
            channel,
            quote,
            q => q.QuoteId.ToString(),
            CommonActions.Updated,
            quote.ToString()
        );
    }

    public static void QuoteDeleted(
        this IAuditor auditor,
        ChatUser user,
        ChannelIdentifier channel,
        int quoteId
    )
    {
        auditor.Record(
            user.Identifier,
            channel,
            nameof(Quote),
            quoteId.ToString(),
            CommonActions.Deleted
        );
    }
}
