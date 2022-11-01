namespace FloppyBot.Commands.Aux.Quotes.Storage;

public interface IQuoteChannelMappingService
{
    string? GetQuoteChannelMapping(string channelId, bool createIfMissing = false);
}
