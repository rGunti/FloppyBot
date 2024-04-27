using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FloppyBot.WebApi.Auth.Controllers;

/// <summary>
/// A base controller class for channel-scoped actions. They usually include a message interface and a channel in its route.
/// </summary>
public abstract class ChannelScopedController : ControllerBase
{
    protected IUserService UserService { get; }

    protected ChannelScopedController(IUserService userService)
    {
        UserService = userService;
    }

    /// <summary>
    /// Ensures that the user has access to the specified channel.
    /// If the user doesn't have access, a <see cref="NotFoundException"/> is thrown, which should result in a 404 Not Found response.
    /// If the user has access, a <see cref="ChannelIdentifier"/> coresponing to the specified message interface and channel is returned.
    /// </summary>
    /// <param name="messageInterface"></param>
    /// <param name="channel"></param>
    /// <returns></returns>
    protected ChannelIdentifier EnsureChannelAccess(string messageInterface, string channel)
    {
        var channelId = new ChannelIdentifier(messageInterface, channel);
        EnsureChannelAccess(channelId);
        return channelId;
    }

    private void EnsureChannelAccess(ChannelIdentifier channelIdentifier)
    {
        if (
            !UserService
                .GetAccessibleChannelsForUser(User.GetUserId())
                .Contains(channelIdentifier.ToString())
        )
        {
            throw new NotFoundException(
                $"You don't have access to {channelIdentifier} or it doesn't exist"
            );
        }
    }
}
