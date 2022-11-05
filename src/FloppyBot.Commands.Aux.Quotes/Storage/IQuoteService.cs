using FloppyBot.Commands.Aux.Quotes.Storage.Entities;

namespace FloppyBot.Commands.Aux.Quotes.Storage;

public interface IQuoteService
{
    Quote? GetRandomQuote(string channelId);
    Quote? GetQuote(string channelId, int quoteId);

    Quote AddQuote(
        string channelId,
        string quoteText,
        string? context,
        string author);

    Quote? EditQuote(string channelId, int quoteId, string newContent);
    Quote? EditQuoteContext(string channelId, int quoteId, string newContext);
    bool DeleteQuote(string channelId, int quoteId);
}
