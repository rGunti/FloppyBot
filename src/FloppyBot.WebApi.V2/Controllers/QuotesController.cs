using FloppyBot.Commands.Aux.Quotes.Storage;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.Controllers;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.V2.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V2.Controllers;

[ApiController]
[Route("api/v2/quotes/{messageInterface}/{channel}")]
[Authorize(Policy = Permissions.READ_QUOTES)]
public class QuotesController : ChannelScopedController
{
    private readonly IQuoteService _quoteService;

    public QuotesController(IUserService userService, IQuoteService quoteService)
        : base(userService)
    {
        _quoteService = quoteService;
    }

    [HttpGet]
    public ActionResult<QuoteDto> GetQuotesForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        return Ok(_quoteService.GetQuotes(channelId).Select(QuoteDto.FromQuote).ToArray());
    }

    [HttpPut("{quoteId:int}")]
    public IActionResult EditQuote(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] int quoteId,
        [FromBody] QuoteDto quote
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        if (!_quoteService.UpdateQuote(channelId, quoteId, quote.ToEntity()))
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{quoteId:int}")]
    public IActionResult DeleteQuote(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] int quoteId
    )
    {
        var channelId = EnsureChannelAccess(messageInterface, channel);
        if (!_quoteService.DeleteQuote(channelId, quoteId))
        {
            return NotFound();
        }

        return NoContent();
    }
}
