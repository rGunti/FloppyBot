using FloppyBot.Commands.Aux.Quotes.Storage.Entities;

namespace FloppyBot.Commands.Aux.Quotes.Storage;

public interface IQuoteService
{
    Quote? GetRandomQuote(string channelId);
    Quote? GetQuote(string channelId, int quoteId);

    Quote AddQuote(string channelId, string quoteText, string? context, string author);

    Quote ImportQuote(Quote quote);

    Quote? EditQuote(string channelId, int quoteId, string newContent);
    Quote? EditQuoteContext(string channelId, int quoteId, string newContext);
    bool DeleteQuote(string channelId, int quoteId);

    IEnumerable<Quote> GetQuotes(string channelId);
    bool UpdateQuote(string channelId, int quoteId, Quote quote);
}
