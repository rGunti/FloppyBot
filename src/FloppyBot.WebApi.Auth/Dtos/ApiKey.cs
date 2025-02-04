using FloppyBot.Base.Storage;

namespace FloppyBot.WebApi.Auth.Dtos;

public record ApiKey(
    string Id,
    string OwnedByUser,
    string Token,
    bool Disabled,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DisabledAt
) : IEntity<ApiKey>
{
    public ApiKey WithId(string newId)
    {
        return this with { Id = newId };
    }
}
