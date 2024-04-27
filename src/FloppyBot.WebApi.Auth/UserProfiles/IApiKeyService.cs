using FloppyBot.Base.Storage;
using FloppyBot.WebApi.Auth.Dtos;

namespace FloppyBot.WebApi.Auth.UserProfiles;

public interface IApiKeyService
{
    void CreateApiKeyForUser(string userId);
    ApiKey? GetApiKeyForUser(string userId);
    ApiKey? GetApiKey(string key);

    bool ValidateApiKeyForUser(string userId, string key);
    void InvalidateApiKeysForUser(string userId);
}

public class ApiKeyService : IApiKeyService
{
    private readonly IRepository<ApiKey> _repository;
    private readonly TimeProvider _timeProvider;

    public ApiKeyService(IRepositoryFactory repositoryFactory, TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _repository = repositoryFactory.GetRepository<ApiKey>();
    }

    public void CreateApiKeyForUser(string userId)
    {
        if (GetApiKeyForUser(userId) is not null)
        {
            return;
        }

        var apiKey = new ApiKey(
            null!,
            userId,
            Guid.NewGuid().ToString(),
            false,
            _timeProvider.GetUtcNow(),
            null
        );
        _repository.Insert(apiKey);
    }

    public ApiKey? GetApiKeyForUser(string userId)
    {
        return _repository.GetAll().FirstOrDefault(k => k.OwnedByUser == userId && !k.Disabled);
    }

    public ApiKey? GetApiKey(string key)
    {
        return _repository.GetAll().FirstOrDefault(k => k.Token == key && !k.Disabled);
    }

    public bool ValidateApiKeyForUser(string userId, string key)
    {
        return _repository
            .GetAll()
            .Any(k => k.OwnedByUser == userId && k.Token == key && !k.Disabled);
    }

    public void InvalidateApiKeysForUser(string userId)
    {
        var apiKeys = _repository
            .GetAll()
            .Where(k => k.OwnedByUser == userId && !k.Disabled)
            .Select(k => k with { Disabled = true, DisabledAt = _timeProvider.GetUtcNow() })
            .ToList();
        if (apiKeys.Count != 0)
        {
            apiKeys.ForEach(k => _repository.Update(k));
        }
    }
}
