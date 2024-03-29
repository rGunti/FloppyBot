using FloppyBot.Base.Storage;

namespace FloppyBot.WebApi.Auth.Dtos;

public record ApiKey(string Id, string OwnedByChannelId, string Token, bool Disabled)
    : IEntity<ApiKey>
{
    public ApiKey WithId(string newId)
    {
        return this with { Id = newId, };
    }
}
