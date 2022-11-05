namespace FloppyBot.Commands.Aux.Quotes.Storage;

public interface IQuoteChannelMappingService
{
    string? GetQuoteChannelMapping(string channelId, bool createIfMissing = false);
    string? StartJoinProcess(string mappingId, string newChannelId);
    bool ConfirmJoinProcess(string mappingId, string newChannelId, string joinCode);
}
