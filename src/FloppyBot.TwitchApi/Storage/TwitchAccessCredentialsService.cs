using FloppyBot.Base.Encryption;
using FloppyBot.Base.Extensions;
using FloppyBot.Base.Storage;
using FloppyBot.TwitchApi.Storage.Entities;

namespace FloppyBot.TwitchApi.Storage;

public interface ITwitchAccessCredentialsService
{
    NullableObject<TwitchAccessCredentials> GetAccessCredentialsFor(string channelName);
    TwitchAccessCredentials StoreAccessCredentials(TwitchAccessCredentials credentials);
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

    public NullableObject<TwitchAccessCredentials> GetAccessCredentialsFor(string channelName)
    {
        return _repository
            .GetAll()
            .FirstOrDefault(c => c.ChannelName == channelName)
            .Wrap()
            .Select(c => _encryptionShim.DecryptSync(c))
            .Wrap();
    }

    public TwitchAccessCredentials StoreAccessCredentials(TwitchAccessCredentials credentials)
    {
        return _repository.Upsert(_encryptionShim.EncryptSync(credentials));
    }
}
