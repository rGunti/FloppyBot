using System.Collections.Immutable;
using FloppyBot.Base.Extensions;
using FloppyBot.Chat.Twitch.Events;
using FloppyBot.Commands.Custom.Storage;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Aux.Twitch.Helpers;

public interface ITwitchRewardConverter
{
    NullableObject<CommandInstruction> ConvertToCommandInstruction(
        TwitchChannelPointsRewardRedeemedEvent rewardRedeemedEvent
    );
}

public class TwitchRewardConverter : ITwitchRewardConverter
{
    private readonly ILogger<TwitchRewardConverter> _logger;
    private readonly ICustomCommandService _commandService;

    public TwitchRewardConverter(
        ILogger<TwitchRewardConverter> logger,
        ICustomCommandService commandService
    )
    {
        _logger = logger;
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
            .Tap(
                command =>
                    _logger.LogDebug(
                        "Found command {CommandName} for reward {RewardId}",
                        command.Name,
                        rewardRedeemedEvent.Reward.RewardId
                    ),
                () =>
                    _logger.LogDebug(
                        "No command found for reward {RewardId}",
                        rewardRedeemedEvent.Reward.RewardId
                    )
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
