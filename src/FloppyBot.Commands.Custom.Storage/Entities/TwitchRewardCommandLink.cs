using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Custom.Storage.Entities;

public record TwitchRewardCommandLink(string Id, CustomCommandDescription Command)
    : IEntity<TwitchRewardCommandLink>
{
    public TwitchRewardCommandLink WithId(string newId)
    {
        return this with { Id = newId };
    }

    public static TwitchRewardCommandLink Create(
        string rewardId,
        CustomCommandDescription customCommandDescription
    )
    {
        return new TwitchRewardCommandLink(rewardId, customCommandDescription);
    }
}
