using FloppyBot.Base.Clock;
using FloppyBot.Base.Extensions;
using FloppyBot.Base.Storage;
using FloppyBot.TwitchApi.Storage.Entities;

namespace FloppyBot.TwitchApi.Storage;

public interface ITwitchAccessCredentialInitiationService
{
    TwitchAccessCredentialInitiation GetOrCreateFor(
        string user,
        string channel,
        IEnumerable<string> withScopes
    );
    NullableObject<TwitchAccessCredentialInitiation> GetFor(
        string user,
        string channel,
        IEnumerable<string> withScopes
    );
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

    public TwitchAccessCredentialInitiation GetOrCreateFor(
        string user,
        string channel,
        IEnumerable<string> withScopes
    )
    {
        var scopes = withScopes.ToArray();
        return GetFor(user, channel, scopes)
            .Or(() =>
                _repository.Insert(
                    new TwitchAccessCredentialInitiation(
                        Guid.NewGuid().ToString(),
                        channel,
                        user,
                        scopes,
                        _timeProvider.GetCurrentUtcTime()
                    )
                )
            );
    }

    public NullableObject<TwitchAccessCredentialInitiation> GetFor(
        string user,
        string channel,
        IEnumerable<string> withScopes
    )
    {
        var initiation = _repository
            .GetAll()
            .FirstOrDefault(i => i.ByUser == user && i.ForChannel == channel);

        if (initiation?.WithScopes is null || initiation.WithScopes.Length == 0)
        {
            // No scopes were provided, consider the request dead
            return NullableObject.Empty<TwitchAccessCredentialInitiation>();
        }

        if (withScopes.Except(initiation.WithScopes).Any())
        {
            // Some scopes are missing, consider the request dead
            return NullableObject.Empty<TwitchAccessCredentialInitiation>();
        }

        return initiation;
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
