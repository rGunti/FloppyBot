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
    // ReSharper disable once UnusedMember.Global
    public QuoteChannelMapping() : this(string.Empty, Array.Empty<string>(), true)
    {
    }

    public QuoteChannelMapping WithId(string newId)
    {
        return this with
        {
            Id = newId
        };
    }
}
