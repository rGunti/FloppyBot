using FloppyBot.Base.Storage;

namespace FloppyBot.TwitchApi.Storage.Entities;

public record TwitchAccessCredentials(
    string Id,
    string ChannelName,
    string AccessToken,
    string RefreshToken,
    string[] WithScopes,
    DateTimeOffset ExpiresOn
) : IEntity<TwitchAccessCredentials>
{
    public TwitchAccessCredentials WithId(string newId)
    {
        return this with { Id = newId };
    }
}

public record TwitchAccessCredentialInitiation(
    string Id,
    string ForChannel,
    string ByUser,
    string[] WithScopes,
    DateTimeOffset CreatedAt
) : IEntity<TwitchAccessCredentialInitiation>
{
    public TwitchAccessCredentialInitiation WithId(string newId)
    {
        return this with { Id = newId };
    }
}
