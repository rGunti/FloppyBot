using System.Collections.Immutable;
using FloppyBot.Base.Extensions;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Twitch.Events;
using FloppyBot.Commands.Custom.Storage;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Aux.Twitch.Helpers;

public interface ITwitchRewardConverter
{
    NullableObject<CommandInstruction> ConvertToCommandInstruction(
        TwitchChannelPointsRewardRedeemedEvent rewardRedeemedEvent,
        ChatMessage sourceMessage
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
        TwitchChannelPointsRewardRedeemedEvent rewardRedeemedEvent,
        ChatMessage sourceMessage
    )
    {
        string channelId = sourceMessage.Identifier.ToChannelIdentifier();
        return _commandService
            .GetCommandForReward(rewardRedeemedEvent.Reward.RewardId, channelId)
            .Tap(
                command =>
                    _logger.LogDebug(
                        "Found command {CommandName} for reward {RewardId} and channel {ChannelId}",
                        command.Name,
                        rewardRedeemedEvent.Reward.RewardId,
                        channelId
                    ),
                () =>
                    _logger.LogDebug(
                        "No command found for reward {RewardId} and {ChannelId}",
                        rewardRedeemedEvent.Reward.RewardId,
                        channelId
                    )
            )
            .Select(x => ConvertToCommandInstruction(x, sourceMessage));
    }

    private static CommandInstruction ConvertToCommandInstruction(
        CustomCommandDescription commandDescription,
        ChatMessage sourceMessage
    )
    {
        return new CommandInstruction(
            commandDescription.Name,
            ImmutableArray<string>.Empty,
            new CommandContext(
                sourceMessage with
                {
                    Author = sourceMessage.Author with
                    {
                        PrivilegeLevel = PrivilegeLevel.Superuser,
                    },
                }
            )
        );
    }
}
