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
            .FirstOrDefault(m => m.ChannelIds.Contains(channelId));

        if (mapping == null && createIfMissing)
        {
            mapping = CreateMapping(channelId);
        }

        return mapping?.Id;
    }

    private QuoteChannelMapping CreateMapping(string channelId)
    {
        var mapping = new QuoteChannelMapping(
            null!,
            new[] { channelId },
            false);
        return _repository.Insert(mapping);
    }
}
