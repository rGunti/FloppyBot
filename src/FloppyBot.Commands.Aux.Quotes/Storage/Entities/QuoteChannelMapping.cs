using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Aux.Quotes.Storage.Entities;

/// <summary>
/// 
/// </summary>
/// <remarks>cannot be compared due to use of arrays</remarks>
/// <param name="Id"></param>
/// <param name="ChannelIds"></param>
/// <param name="NeedsConfirmation"></param>
public record QuoteChannelMapping(
    string Id,
    string[] ChannelIds,
    bool NeedsConfirmation) : IEntity<QuoteChannelMapping>
{
    public QuoteChannelMapping WithId(string newId)
    {
        return this with
        {
            Id = newId
        };
    }
}
