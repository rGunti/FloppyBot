using FloppyBot.Commands.Aux.Twitch.Entities;
using FloppyBot.WebApi.V2.Dtos.CustomCommands;

namespace FloppyBot.WebApi.V2.Dtos.Twitch;

public record TwitchRewardDto(ChannelReward Reward, CustomCommandDto? LinkedCommand);
