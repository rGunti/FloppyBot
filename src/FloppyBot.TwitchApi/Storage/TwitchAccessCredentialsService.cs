using FloppyBot.Base.Encryption;
using FloppyBot.Base.Extensions;
using FloppyBot.Base.Storage;
using FloppyBot.TwitchApi.Storage.Entities;

namespace FloppyBot.TwitchApi.Storage;

public interface ITwitchAccessCredentialsService
{
    bool HasAccessCredentialsFor(string channelName);
    NullableObject<TwitchAccessCredentials> GetAccessCredentialsFor(string channelName);
    NullableObject<TwitchAccessCredentials> GetAccessCredentialsFor(
        string channelName,
        params string[] withScopes
    );
    TwitchAccessCredentials StoreAccessCredentials(TwitchAccessCredentials credentials);
    void DeleteAccessCredentials(string channelName);

    IEnumerable<string> GetAllKnownCredentials();
}

public class TwitchAccessCredentialsService : ITwitchAccessCredentialsService
{
    private readonly IRepository<TwitchAccessCredentials> _repository;
    private readonly IEncryptionShim _encryptionShim;

    public TwitchAccessCredentialsService(
        IRepositoryFactory repositoryFactory,
        IEncryptionShim encryptionShim
    )
    {
        _encryptionShim = encryptionShim;
        _repository = repositoryFactory.GetRepository<TwitchAccessCredentials>();
    }

    public bool HasAccessCredentialsFor(string channelName)
    {
        return _repository.GetById(channelName) is not null;
    }

    public NullableObject<TwitchAccessCredentials> GetAccessCredentialsFor(string channelName)
    {
        return _repository
            .GetById(channelName)
            .Wrap()
            .Select(c => _encryptionShim.DecryptSync(c))
            .Wrap();
    }

    public NullableObject<TwitchAccessCredentials> GetAccessCredentialsFor(
        string channelName,
        params string[] withScopes
    )
    {
        return _repository
            .GetById(channelName)
            .Wrap()
            .Where(c =>
            {
                if (withScopes.Length == 0)
                {
                    return true;
                }

                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (c.WithScopes is null)
                {
                    return false;
                }

                return withScopes.All(s => c.WithScopes.Contains(s));
            })
            .Select(c => _encryptionShim.DecryptSync(c))
            .Wrap();
    }

    public TwitchAccessCredentials StoreAccessCredentials(TwitchAccessCredentials credentials)
    {
        return _repository.Upsert(_encryptionShim.EncryptSync(credentials));
    }

    public void DeleteAccessCredentials(string channelName)
    {
        _repository.Delete(channelName);
    }

    public IEnumerable<string> GetAllKnownCredentials()
    {
        return _repository.GetAll().Select(cred => cred.ChannelName).ToList();
    }
}
