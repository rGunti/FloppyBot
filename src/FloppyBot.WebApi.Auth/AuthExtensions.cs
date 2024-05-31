using System.Security.Claims;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;

namespace FloppyBot.WebApi.Auth;

public static class AuthExtensions
{
    public const string API_KEY_CHANNEL_CLAIM = "floppybot/apiKey/channelId";
    public const string USER_ID_CLAIM =
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

    private const string USER_PERMISSION_CLAIM = "permissions";

    public static string GetUserId(this ClaimsPrincipal user)
    {
        return user.Identities.First().Claims.First(c => c.Type == USER_ID_CLAIM).Value;
    }

    public static string? TryGetUserId(this ClaimsPrincipal user)
    {
        return user.Identities.First().Claims.FirstOrDefault(c => c.Type == USER_ID_CLAIM)?.Value;
    }

    public static IEnumerable<string> GetUserPermissions(this ClaimsPrincipal user)
    {
        return user
            .Identities.First()
            .Claims.Where(c => c.Type == USER_PERMISSION_CLAIM)
            .Select(c => c.Value);
    }

    public static string? GetChannelIdForApiKeyUser(this ClaimsPrincipal user)
    {
        return user
            .Identities.SelectMany(i => i.Claims)
            .Where(c => c.Type == API_KEY_CHANNEL_CLAIM)
            .Select(c => c.Value)
            .FirstOrDefault();
    }

    public static ChatUser AsChatUser(this ClaimsPrincipal user)
    {
        return new ChatUser(
            new ChannelIdentifier("WebApi", user.GetUserId()),
            string.Empty,
            PrivilegeLevel.Unknown
        );
    }
}
