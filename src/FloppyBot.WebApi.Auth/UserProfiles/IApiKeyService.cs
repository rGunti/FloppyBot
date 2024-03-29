using FloppyBot.Base.Storage;
using FloppyBot.WebApi.Auth.Dtos;

namespace FloppyBot.WebApi.Auth.UserProfiles;

public interface IApiKeyService
{
    void CreateApiKeyForChannel(string channelId);
    ApiKey? GetApiKeyForChannel(string channelId);
    ApiKey? GetApiKey(string key);

    bool ValidateApiKeyForChannel(string channelId, string key);
}

public class ApiKeyService : IApiKeyService
{
    private readonly IRepository<ApiKey> _repository;

    public ApiKeyService(IRepositoryFactory repositoryFactory)
    {
        _repository = repositoryFactory.GetRepository<ApiKey>();
    }

    public void CreateApiKeyForChannel(string channelId)
    {
        if (GetApiKeyForChannel(channelId) is not null)
        {
            return;
        }

        var apiKey = new ApiKey(null!, channelId, Guid.NewGuid().ToString(), false);
        _repository.Insert(apiKey);
    }

    public ApiKey? GetApiKeyForChannel(string channelId)
    {
        return _repository
            .GetAll()
            .FirstOrDefault(k => k.OwnedByChannelId == channelId && !k.Disabled);
    }

    public ApiKey? GetApiKey(string key)
    {
        return _repository.GetAll().FirstOrDefault(k => k.Token == key && !k.Disabled);
    }

    public bool ValidateApiKeyForChannel(string channelId, string key)
    {
        return _repository.GetAll().Any(k => k.OwnedByChannelId == channelId && k.Token == key);
    }
}
