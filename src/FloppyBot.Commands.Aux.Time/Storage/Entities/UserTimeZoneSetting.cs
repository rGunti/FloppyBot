using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Aux.Time.Storage.Entities;

public record UserTimeZoneSetting(
    string Id,
    string TimeZoneId) : IEntity<UserTimeZoneSetting>
{
    public UserTimeZoneSetting WithId(string newId)
    {
        return this with
        {
            Id = newId
        };
    }
}

