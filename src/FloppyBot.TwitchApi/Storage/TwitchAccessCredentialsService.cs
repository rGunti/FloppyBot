using FloppyBot.Base.Encryption;
using FloppyBot.Base.Extensions;
using FloppyBot.Base.Storage;
using FloppyBot.TwitchApi.Storage.Entities;

namespace FloppyBot.TwitchApi.Storage;

public interface ITwitchAccessCredentialsService
{
    bool HasAccessCredentialsFor(string channelName);
    NullableObject<TwitchAccessCredentials> GetAccessCredentialsFor(string channelName);
    TwitchAccessCredentials StoreAccessCredentials(TwitchAccessCredentials credentials);
    void DeleteAccessCredentials(string channelName);
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

    public TwitchAccessCredentials StoreAccessCredentials(TwitchAccessCredentials credentials)
    {
        return _repository.Upsert(_encryptionShim.EncryptSync(credentials));
    }

    public void DeleteAccessCredentials(string channelName)
    {
        _repository.Delete(channelName);
    }
}
