using FloppyBot.Base.Clock;
using FloppyBot.Base.Extensions;
using FloppyBot.Base.Storage;
using FloppyBot.TwitchApi.Storage.Entities;

namespace FloppyBot.TwitchApi.Storage;

public interface ITwitchAccessCredentialInitiationService
{
    TwitchAccessCredentialInitiation GetOrCreateFor(string user, string channel);
    NullableObject<TwitchAccessCredentialInitiation> GetFor(string user, string channel);
    NullableObject<TwitchAccessCredentialInitiation> GetForSessionId(string sessionId);
    void DeleteFor(string user, string channel);
    void Delete(TwitchAccessCredentialInitiation initiation);
}

public class TwitchAccessCredentialInitiationService : ITwitchAccessCredentialInitiationService
{
    private readonly IRepository<TwitchAccessCredentialInitiation> _repository;
    private readonly ITimeProvider _timeProvider;

    public TwitchAccessCredentialInitiationService(
        IRepositoryFactory repositoryFactory,
        ITimeProvider timeProvider
    )
    {
        _timeProvider = timeProvider;
        _repository = repositoryFactory.GetRepository<TwitchAccessCredentialInitiation>();
    }

    public TwitchAccessCredentialInitiation GetOrCreateFor(string user, string channel)
    {
        return GetFor(user, channel)
            .Or(
                () =>
                    _repository.Insert(
                        new TwitchAccessCredentialInitiation(
                            Guid.NewGuid().ToString(),
                            channel,
                            user,
                            _timeProvider.GetCurrentUtcTime()
                        )
                    )
            );
    }

    public NullableObject<TwitchAccessCredentialInitiation> GetFor(string user, string channel)
    {
        return _repository
            .GetAll()
            .FirstOrDefault(i => i.ByUser == user && i.ForChannel == channel);
    }

    public NullableObject<TwitchAccessCredentialInitiation> GetForSessionId(string sessionId)
    {
        return _repository.GetAll().FirstOrDefault(i => i.Id == sessionId).Wrap();
    }

    public void DeleteFor(string user, string channel)
    {
        _repository.Delete(
            _repository
                .GetAll()
                .Where(i => i.ByUser == user && i.ForChannel == channel)
                .Select(i => i.Id)
        );
    }

    public void Delete(TwitchAccessCredentialInitiation initiation)
    {
        _repository.Delete(initiation);
    }
}
