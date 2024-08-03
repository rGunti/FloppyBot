using FloppyBot.Base.Clock;
using FloppyBot.Base.Extensions;
using FloppyBot.Base.Storage;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Aux.Currency.Storage.Entities;

namespace FloppyBot.Commands.Aux.Currency.Storage;

public interface IChannelUserStateService
{
    NullableObject<ChannelUserState> GetStateForUserInChannel(
        ChannelIdentifier userIdentifier,
        ChannelIdentifier channelIdentifier
    );
    ChannelUserState MarkOnline(
        ChannelIdentifier userIdentifier,
        ChannelIdentifier channelIdentifier
    );
    ChannelUserState MarkOffline(
        ChannelIdentifier userIdentifier,
        ChannelIdentifier channelIdentifier
    );
}

public class ChannelUserStateService : IChannelUserStateService
{
    private readonly IRepository<ChannelUserState> _repository;
    private readonly ITimeProvider _timeProvider;

    public ChannelUserStateService(IRepositoryFactory repositoryFactory, ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _repository = repositoryFactory.GetRepository<ChannelUserState>();
    }

    public NullableObject<ChannelUserState> GetStateForUserInChannel(
        ChannelIdentifier userIdentifier,
        ChannelIdentifier channelIdentifier
    )
    {
        return _repository
            .GetById(ChannelUserState.GetId(userIdentifier, channelIdentifier))
            .Wrap();
    }

    public ChannelUserState MarkOnline(
        ChannelIdentifier userIdentifier,
        ChannelIdentifier channelIdentifier
    )
    {
        var state = GetStateForUserInChannel(userIdentifier, channelIdentifier)
            .FirstOrDefault(
                ChannelUserState.ForUserInChannel(userIdentifier, channelIdentifier)
            ) with
        {
            State = UserState.Online,
            LastStateChange = _timeProvider.GetCurrentUtcTime(),
        };

        return _repository.Upsert(state);
    }

    public ChannelUserState MarkOffline(
        ChannelIdentifier userIdentifier,
        ChannelIdentifier channelIdentifier
    )
    {
        var state = GetStateForUserInChannel(userIdentifier, channelIdentifier)
            .FirstOrDefault(
                ChannelUserState.ForUserInChannel(userIdentifier, channelIdentifier)
            ) with
        {
            State = UserState.Offline,
            LastStateChange = _timeProvider.GetCurrentUtcTime(),
        };

        return _repository.Upsert(state);
    }
}
