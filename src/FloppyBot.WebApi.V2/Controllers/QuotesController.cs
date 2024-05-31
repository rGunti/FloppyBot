using FloppyBot.Base.Auditing.Abstraction;
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
    private readonly IAuditor _auditor;

    public QuotesController(IUserService userService, IQuoteService quoteService, IAuditor auditor)
        : base(userService)
    {
        _quoteService = quoteService;
        _auditor = auditor;
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
        var quoteEntity = quote.ToEntity();
        if (!_quoteService.UpdateQuote(channelId, quoteId, quoteEntity))
        {
            return NotFound();
        }

        _auditor.QuoteUpdated(User.AsChatUser(), channelId, quoteEntity);
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

        _auditor.QuoteDeleted(User.AsChatUser(), channelId, quoteId);
        return NoContent();
    }
}
