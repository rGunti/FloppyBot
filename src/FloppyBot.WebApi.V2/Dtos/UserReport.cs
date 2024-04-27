using FloppyBot.WebApi.Auth.Dtos;

namespace FloppyBot.WebApi.V2.Dtos;

public record UserReport(
    string UserId,
    List<string> OwnerOf,
    Dictionary<string, string> ChannelAliases,
    string[] Permissions,
    DateTimeOffset? ApiKeyCreatedAt
)
{
    public static UserReport FromUser(
        User user,
        string[] permissions,
        DateTimeOffset? apiKeyCreatedAt
    )
    {
        return new UserReport(
            user.Id,
            user.OwnerOf,
            user.ChannelAliases,
            permissions,
            apiKeyCreatedAt
        );
    }
}
