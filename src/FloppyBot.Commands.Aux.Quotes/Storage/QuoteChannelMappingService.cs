using FloppyBot.Base.Storage;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;

namespace FloppyBot.Commands.Aux.Quotes.Storage;

public class QuoteChannelMappingService : IQuoteChannelMappingService
{
    private readonly IRepository<QuoteChannelMapping> _repository;

    public QuoteChannelMappingService(IRepositoryFactory repositoryFactory)
    {
        _repository = repositoryFactory.GetRepository<QuoteChannelMapping>();
    }

    public string? GetQuoteChannelMapping(string channelId, bool createIfMissing = false)
    {
        var mapping = _repository
            .GetAll()
            .FirstOrDefault(m => m.ChannelId == channelId && m.Confirmed);

        if (mapping == null && createIfMissing)
        {
            mapping = CreateNewMapping(channelId);
        }

        return mapping?.MappingId;
    }

    private QuoteChannelMapping CreateNewMapping(string channelId)
    {
        var mapping = new QuoteChannelMapping(
            null!,
            Guid.NewGuid().ToString(),
            channelId,
            true);
        return _repository.Insert(mapping);
    }
}
