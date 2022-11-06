using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.v1;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/quotes")]
public class QuotesController : ControllerBase
{
    [HttpGet]
    public QuoteDto[] GetQuotes()
    {
        throw this.NotImplemented();
    }

    [HttpGet("{messageInterface}/{channel}")]
    public QuoteDto[] GetQuotesForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel)
    {
        throw this.NotImplemented();
    }

    [HttpGet("{messageInterface}/{channel}/{quoteNumber}")]
    public QuoteDto GetQuoteForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] int quoteNumber)
    {
        throw this.NotImplemented();
    }

    [HttpPut("{messageInterface}/{channel}/{quoteNumber}")]
    public IActionResult UpdateQuote(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] int quoteNumber,
        [FromBody] QuoteDto updatedQuote)
    {
        throw this.NotImplemented();
    }

    [HttpDelete("{messageInterface}/{channel}/{quoteNumber}")]
    public IActionResult DeleteQuote(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] int quoteNumber)
    {
        throw this.NotImplemented();
    }
}
