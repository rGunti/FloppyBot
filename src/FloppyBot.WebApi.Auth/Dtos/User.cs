using FloppyBot.Base.Storage;

namespace FloppyBot.WebApi.Auth.Dtos;

public record User(string Id, List<string> OwnerOf, Dictionary<string, string> ChannelAliases)
    : IEntity<User>
{
    public User WithId(string newId)
    {
        return this with { Id = newId };
    }
}
