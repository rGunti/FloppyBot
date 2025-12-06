using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Custom.Storage.Entities.Internal;

public record TwitchRewardCommandLinkEo(string Id, string ChannelId, string CommandId)
    : IEntity<TwitchRewardCommandLinkEo>
{
    public TwitchRewardCommandLinkEo WithId(string newId)
    {
        return this with { Id = newId };
    }
}
