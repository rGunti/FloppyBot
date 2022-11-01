using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.Attributes;

namespace FloppyBot.Commands.Aux.Twitch.Storage.Entities;

[CollectionName("ShoutoutMessageSettings")]
public record ShoutoutMessageSetting(
    string Id,
    string Message) : IEntity<ShoutoutMessageSetting>
{
    public ShoutoutMessageSetting WithId(string newId)
    {
        return this with
        {
            Id = newId
        };
    }
}
