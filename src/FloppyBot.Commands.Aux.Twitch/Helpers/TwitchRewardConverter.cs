using System.Collections.Immutable;
using FloppyBot.Base.Extensions;
using FloppyBot.Chat.Twitch.Events;
using FloppyBot.Commands.Custom.Storage;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Aux.Twitch.Helpers;

public interface ITwitchRewardConverter
{
    NullableObject<CommandInstruction> ConvertToCommandInstruction(
        TwitchChannelPointsRewardRedeemedEvent rewardRedeemedEvent
    );
}

public class TwitchRewardConverter : ITwitchRewardConverter
{
    private readonly ICustomCommandService _commandService;

    public TwitchRewardConverter(ICustomCommandService commandService)
    {
        _commandService = commandService;
    }

    public NullableObject<CommandInstruction> ConvertToCommandInstruction(
        TwitchChannelPointsRewardRedeemedEvent rewardRedeemedEvent
    )
    {
        return _commandService
            .GetCommandForReward(
                rewardRedeemedEvent.Reward.RewardId,
                rewardRedeemedEvent.User.Identifier
            )
            .Select(ConvertToCommandInstruction);
    }

    private static CommandInstruction ConvertToCommandInstruction(
        CustomCommandDescription commandDescription
    )
    {
        return new CommandInstruction(commandDescription.Name, ImmutableArray<string>.Empty);
    }
}
