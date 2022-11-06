using System.Collections.Immutable;
using FloppyBot.Base.EquatableCollections;
using FloppyBot.Base.Storage;
using FloppyBot.WebApi.Auth.Dtos;

namespace FloppyBot.WebApi.Auth.UserProfiles;

public interface IUserService
{
    IImmutableList<string> GetAccessibleChannelsForUser(string userId);
    User? GetUserInfo(string userId, bool createIfMissing = false);
}

public class UserService : IUserService
{
    private readonly IRepository<User> _repository;

    public UserService(IRepositoryFactory repositoryFactory)
    {
        _repository = repositoryFactory.GetRepository<User>();
    }

    public IImmutableList<string> GetAccessibleChannelsForUser(string userId)
    {
        var ownerList = GetUserInfo(userId)?.OwnerOf ?? Enumerable.Empty<string>();
        return ownerList.ToImmutableListWithValueSemantics();
    }

    public User? GetUserInfo(string userId, bool createIfMissing = false)
    {
        var user = _repository.GetById(userId);
        if (user == null && createIfMissing)
        {
            user = new User(
                userId,
                new List<string>(),
                new Dictionary<string, string>());
            user = _repository.Insert(user);
        }

        if (user != null)
        {
            // Ensure all channel IDs are available as channel alias
            user = user with
            {
                ChannelAliases = user.OwnerOf
                    .ToDictionary(
                        c => c,
                        c => user.ChannelAliases.GetValueOrDefault(c) ?? "")
            };
        }

        return user;
    }
}
