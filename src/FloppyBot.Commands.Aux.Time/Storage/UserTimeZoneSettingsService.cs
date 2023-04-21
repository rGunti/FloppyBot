using FloppyBot.Base.Extensions;
using FloppyBot.Base.Storage;
using FloppyBot.Commands.Aux.Time.Storage.Entities;

namespace FloppyBot.Commands.Aux.Time.Storage;

public interface IUserTimeZoneSettingsService
{
    NullableObject<UserTimeZoneSetting> GetTimeZoneForUser(string userId);
    void SetTimeZoneForUser(string userId, string timezoneId);
}

public class UserTimeZoneSettingsService : IUserTimeZoneSettingsService
{
    private readonly IRepository<UserTimeZoneSetting> _repository;

    public UserTimeZoneSettingsService(IRepositoryFactory repositoryFactory)
    {
        _repository = repositoryFactory.GetRepository<UserTimeZoneSetting>();
    }

    public NullableObject<UserTimeZoneSetting> GetTimeZoneForUser(string userId)
    {
        return _repository.GetById(userId).Wrap();
    }

    public void SetTimeZoneForUser(string userId, string timezoneId)
    {
        NullableObject<UserTimeZoneSetting> setting = GetTimeZoneForUser(userId);
        if (setting.HasValue)
        {
            _repository.Update(setting.Value with { TimeZoneId = timezoneId });
        }
        else
        {
            _repository.Insert(new UserTimeZoneSetting(userId, timezoneId));
        }
    }
}
