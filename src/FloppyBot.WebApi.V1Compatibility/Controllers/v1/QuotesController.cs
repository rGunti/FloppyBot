using AutoMapper;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Aux.Quotes.Storage;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/quotes")]
public class QuotesController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IQuoteService _quoteService;

    public QuotesController(IQuoteService quoteService, IMapper mapper)
    {
        _quoteService = quoteService;
        _mapper = mapper;
    }

    [HttpGet]
    public QuoteDto[] GetQuotes()
    {
        // TODO: Figure out what channels this user has access to
        throw this.NotImplemented();
    }

    [HttpGet("{messageInterface}/{channel}")]
    public QuoteDto[] GetQuotesForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel)
    {
        var channelIdentifier = new ChannelIdentifier(messageInterface, channel);
        return _quoteService
            .GetQuotes(channelIdentifier)
            .Select(q => _mapper.Map<QuoteDto>(q) with
            {
                Channel = channelIdentifier
            })
            .ToArray();
    }

    [HttpGet("{messageInterface}/{channel}/{quoteNumber}")]
    public QuoteDto GetQuoteForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] int quoteNumber)
    {
        var channelIdentifier = new ChannelIdentifier(messageInterface, channel);
        var quote = _quoteService.GetQuote(channelIdentifier, quoteNumber);
        if (quote == null)
        {
            throw new NotFoundException($"Quote {channelIdentifier}#{quoteNumber} not found");
        }

        return _mapper.Map<QuoteDto>(quote) with { Channel = channelIdentifier };
    }

    [HttpPut("{messageInterface}/{channel}/{quoteNumber}")]
    public IActionResult UpdateQuote(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] int quoteNumber,
        [FromBody] QuoteDto updatedQuote)
    {
        var channelIdentifier = new ChannelIdentifier(messageInterface, channel);
        var quote = _mapper.Map<Quote>(updatedQuote);

        if (!_quoteService.UpdateQuote(channelIdentifier, quoteNumber, quote))
        {
            throw new BadRequestException($"Failed to update Quote {channelIdentifier}#{quoteNumber}");
        }

        return NoContent();
    }

    [HttpDelete("{messageInterface}/{channel}/{quoteNumber}")]
    public IActionResult DeleteQuote(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] int quoteNumber)
    {
        var channelIdentifier = new ChannelIdentifier(messageInterface, channel);
        if (!_quoteService.DeleteQuote(channelIdentifier, quoteNumber))
        {
            throw new BadRequestException($"Failed to delete Quote {channelIdentifier}#{quoteNumber}");
        }

        return NoContent();
    }
}
