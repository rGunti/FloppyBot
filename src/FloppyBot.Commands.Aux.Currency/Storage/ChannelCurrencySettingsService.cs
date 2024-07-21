using FloppyBot.Base.Extensions;
using FloppyBot.Base.Storage;
using FloppyBot.Commands.Aux.Currency.Storage.Entities;

namespace FloppyBot.Commands.Aux.Currency.Storage;

public interface IChannelCurrencySettingsService
{
    NullableObject<ChannelCurrencySettings> GetChannelCurrencySettings(string channel);
    ChannelCurrencySettings SetChannelCurrencySettings(ChannelCurrencySettings settings);
    void DisableChannelCurrencySettings(string channel);
}

public class ChannelCurrencySettingsService : IChannelCurrencySettingsService
{
    private readonly IRepository<ChannelCurrencySettings> _repository;

    public ChannelCurrencySettingsService(IRepositoryFactory repositoryFactory)
    {
        _repository = repositoryFactory.GetRepository<ChannelCurrencySettings>();
    }

    public NullableObject<ChannelCurrencySettings> GetChannelCurrencySettings(string channel)
    {
        return _repository.GetById(channel).Wrap();
    }

    public ChannelCurrencySettings SetChannelCurrencySettings(ChannelCurrencySettings settings)
    {
        var existingSettings = GetChannelCurrencySettings(settings.Id);
        if (existingSettings.HasValue)
        {
            return _repository.Update(settings);
        }

        return _repository.Insert(settings);
    }

    public void DisableChannelCurrencySettings(string channel)
    {
        var existingSettings = GetChannelCurrencySettings(channel);
        if (existingSettings.HasValue)
        {
            _repository.Update(existingSettings.Value with { Enabled = false });
        }
    }
}
