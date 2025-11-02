using FloppyBot.Base.Storage;

namespace FloppyBot.TwitchApi.Storage.Entities;

public record TwitchAccessCredentials(
    string Id,
    string ChannelName,
    // TODO: Encryption
    string AccessToken,
    string RefreshToken,
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
    DateTimeOffset CreatedAt
) : IEntity<TwitchAccessCredentialInitiation>
{
    public TwitchAccessCredentialInitiation WithId(string newId)
    {
        return this with { Id = newId };
    }
}
