using FloppyBot.Base.Clock;
using FloppyBot.Base.Rng;
using FloppyBot.Base.Storage;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;

namespace FloppyBot.Commands.Aux.Quotes.Storage;

public class QuoteService : IQuoteService
{
    private static readonly Random Rng = new(DateTime.UtcNow.Millisecond + DateTime.Now.Minute);
    private readonly IQuoteChannelMappingService _channelMappingService;
    private readonly IRandomNumberGenerator _randomNumberGenerator;

    private readonly IRepository<Quote> _repository;
    private readonly ITimeProvider _timeProvider;

    public QuoteService(
        IRepositoryFactory repositoryFactory,
        IQuoteChannelMappingService channelMappingService,
        ITimeProvider timeProvider,
        IRandomNumberGenerator randomNumberGenerator)
    {
        _channelMappingService = channelMappingService;
        _timeProvider = timeProvider;
        _randomNumberGenerator = randomNumberGenerator;
        _repository = repositoryFactory.GetRepository<Quote>();
    }

    public Quote? GetRandomQuote(string channelId)
    {
        var mappingId = _channelMappingService.GetQuoteChannelMapping(channelId);
        if (mappingId == null)
        {
            return null;
        }

        var maxQuoteId = DetermineNextQuoteId(mappingId);
        if (maxQuoteId == 1)
        {
            return null;
        }

        var retries = 5;
        Quote? quote;
        do
        {
            quote = GetQuoteByChannelMappingId(mappingId, _randomNumberGenerator.Next(1, maxQuoteId));
            retries--;
        } while (quote == null && retries > 0);

        return quote;
    }

    public Quote? GetQuote(string channelId, int quoteId)
    {
        var mappingId = _channelMappingService.GetQuoteChannelMapping(channelId);
        if (mappingId == null)
        {
            return null;
        }

        return GetQuoteByChannelMappingId(mappingId, quoteId);
    }

    public Quote AddQuote(string channelId, string quoteText, string? context, string author)
    {
        var mappingId = _channelMappingService.GetQuoteChannelMapping(channelId, true)!;
        var newQuoteId = DetermineNextQuoteId(mappingId);
        return _repository.Insert(new Quote(
            null!,
            mappingId,
            newQuoteId,
            quoteText,
            context ?? string.Empty,
            _timeProvider.GetCurrentUtcTime(),
            author));
    }

    public Quote ImportQuote(Quote quote)
    {
        var channelMappingId = _channelMappingService.GetQuoteChannelMapping(
            quote.ChannelMappingId,
            true)!;
        return _repository.Insert(quote with
        {
            ChannelMappingId = channelMappingId
        });
    }

    public Quote? EditQuote(string channelId, int quoteId, string newContent)
    {
        var quote = GetQuote(channelId, quoteId);
        if (quote == null)
        {
            return null;
        }

        return _repository.Update(quote with
        {
            QuoteText = newContent
        });
    }

    public Quote? EditQuoteContext(string channelId, int quoteId, string newContext)
    {
        var quote = GetQuote(channelId, quoteId);
        if (quote == null)
        {
            return null;
        }

        return _repository.Update(quote with
        {
            QuoteContext = newContext
        });
    }

    public bool DeleteQuote(string channelId, int quoteId)
    {
        var quote = GetQuote(channelId, quoteId);
        return quote != null && _repository.Delete(quote);
    }

    public IEnumerable<Quote> GetQuotes(string channelId)
    {
        var mapping = _channelMappingService.GetQuoteChannelMapping(channelId);
        if (mapping == null)
        {
            return Enumerable.Empty<Quote>();
        }

        return _repository.GetAll()
            .Where(q => q.ChannelMappingId == mapping);
    }

    public bool UpdateQuote(string channelId, int quoteId, Quote quote)
    {
        var mapping = _channelMappingService.GetQuoteChannelMapping(channelId);
        if (mapping == null)
        {
            return false;
        }

        var existingQuote = GetQuote(channelId, quoteId);
        if (existingQuote == null)
        {
            return false;
        }

        _repository.Update(quote with
        {
            // Force properties to stay as-is
            Id = existingQuote.Id,
            QuoteId = existingQuote.QuoteId,
            ChannelMappingId = existingQuote.ChannelMappingId
        });
        return true;
    }

    private Quote? GetQuoteByChannelMappingId(string mappingId, int quoteId)
    {
        return _repository.GetAll()
            .FirstOrDefault(q => q.ChannelMappingId == mappingId && q.QuoteId == quoteId);
    }

    private int DetermineNextQuoteId(string channelMappingId)
    {
        return DetermineLastQuoteId(channelMappingId) + 1;
    }

    private int DetermineLastQuoteId(string channelMappingId)
    {
        // ReSharper disable once SimplifyLinqExpressionUseAll
        if (!_repository.GetAll().Any(q => q.ChannelMappingId == channelMappingId))
            return 0;

        return _repository.GetAll()
            .Where(q => q.ChannelMappingId == channelMappingId)
            .Max(q => q.QuoteId);
    }
}

