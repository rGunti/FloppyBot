using AutoMapper;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Aux.Quotes.Storage;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.V1Compatibility.Controllers.V1;

[ApiController]
[Route(V1Config.ROUTE_BASE + "api/v1/quotes")]
[Authorize(Policy = Permissions.READ_QUOTES)]
public class QuotesController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IQuoteService _quoteService;
    private readonly IUserService _userService;

    public QuotesController(IQuoteService quoteService, IUserService userService, IMapper mapper)
    {
        _quoteService = quoteService;
        _mapper = mapper;
        _userService = userService;
    }

    [HttpGet]
    [Obsolete("This method will not be implemented as it is not used")]
    public void GetQuotes()
    {
        throw this.Obsolete();
    }

    [HttpGet("{messageInterface}/{channel}")]
    public QuoteDto[] GetQuotesForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel
    )
    {
        var channelIdentifier = new ChannelIdentifier(messageInterface, channel);
        EnsureChannelAccess(channelIdentifier);
        return GetQuotesForChannel(channelIdentifier).ToArray();
    }

    [HttpGet("{messageInterface}/{channel}/{quoteNumber}")]
    public QuoteDto GetQuoteForChannel(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] int quoteNumber
    )
    {
        var channelIdentifier = new ChannelIdentifier(messageInterface, channel);
        EnsureChannelAccess(channelIdentifier);
        var quote = _quoteService.GetQuote(channelIdentifier, quoteNumber);
        if (quote == null)
        {
            throw new NotFoundException($"Quote {channelIdentifier}#{quoteNumber} not found");
        }

        return _mapper.Map<QuoteDto>(quote) with
        {
            Channel = channelIdentifier,
        };
    }

    [HttpPut("{messageInterface}/{channel}/{quoteNumber}")]
    [Authorize(Policy = Permissions.EDIT_QUOTES)]
    public IActionResult UpdateQuote(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] int quoteNumber,
        [FromBody] QuoteDto updatedQuote
    )
    {
        var channelIdentifier = new ChannelIdentifier(messageInterface, channel);
        EnsureChannelAccess(channelIdentifier);
        var quote = _mapper.Map<Quote>(updatedQuote);

        if (!_quoteService.UpdateQuote(channelIdentifier, quoteNumber, quote))
        {
            throw new BadRequestException(
                $"Failed to update Quote {channelIdentifier}#{quoteNumber}"
            );
        }

        return NoContent();
    }

    [HttpDelete("{messageInterface}/{channel}/{quoteNumber}")]
    [Authorize(Policy = Permissions.EDIT_QUOTES)]
    public IActionResult DeleteQuote(
        [FromRoute] string messageInterface,
        [FromRoute] string channel,
        [FromRoute] int quoteNumber
    )
    {
        var channelIdentifier = new ChannelIdentifier(messageInterface, channel);
        EnsureChannelAccess(channelIdentifier);
        if (!_quoteService.DeleteQuote(channelIdentifier, quoteNumber))
        {
            throw new BadRequestException(
                $"Failed to delete Quote {channelIdentifier}#{quoteNumber}"
            );
        }

        return NoContent();
    }

    private void EnsureChannelAccess(ChannelIdentifier channelIdentifier)
    {
        if (
            !_userService
                .GetAccessibleChannelsForUser(User.GetUserId())
                .Contains(channelIdentifier.ToString())
        )
        {
            throw new NotFoundException(
                $"You don't have access to {channelIdentifier} or it doesn't exist"
            );
        }
    }

    private void EnsureChannelAccess(string messageInterface, string channel) =>
        EnsureChannelAccess(new ChannelIdentifier(messageInterface, channel));

    private IEnumerable<QuoteDto> GetQuotesForChannel(ChannelIdentifier channelIdentifier)
    {
        return _quoteService
            .GetQuotes(channelIdentifier)
            .Select(q => _mapper.Map<QuoteDto>(q) with { Channel = channelIdentifier });
    }
}
