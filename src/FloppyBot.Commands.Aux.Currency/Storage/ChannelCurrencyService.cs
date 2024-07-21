using FloppyBot.Base.Extensions;
using FloppyBot.Base.Storage;
using FloppyBot.Commands.Aux.Currency.Storage.Entities;

namespace FloppyBot.Commands.Aux.Currency.Storage;

public interface IChannelCurrencyService
{
    NullableObject<ChannelCurrencyRecord> GetChannelCurrency(string channel, string user);
    ChannelCurrencyRecord IncrementCurrency(string channel, string user, int amount);
    ChannelCurrencyRecord? ClearBalance(string channel, string user);
}

public class ChannelCurrencyService : IChannelCurrencyService
{
    private readonly IRepository<ChannelCurrencyRecord> _repository;

    public ChannelCurrencyService(IRepositoryFactory repositoryFactory)
    {
        _repository = repositoryFactory.GetRepository<ChannelCurrencyRecord>();
    }

    public NullableObject<ChannelCurrencyRecord> GetChannelCurrency(string channel, string user)
    {
        return _repository
            .GetAll()
            .FirstOrDefault(r =>
                r.Channel == channel.ToLowerInvariant() && r.User == user.ToLowerInvariant()
            )
            .Wrap();
    }

    public ChannelCurrencyRecord IncrementCurrency(string channel, string user, int amount)
    {
        var record = GetChannelCurrency(channel, user)
            .Select(r => _repository.IncrementField(r.Id, f => f.Balance, amount))
            .FirstOrDefault();

        if (record is not null)
        {
            return record;
        }

        return _repository.Insert(
            new ChannelCurrencyRecord(
                null!,
                channel.ToLowerInvariant(),
                user.ToLowerInvariant(),
                amount
            )
        );
    }

    public ChannelCurrencyRecord? ClearBalance(string channel, string user)
    {
        var record = GetChannelCurrency(channel, user)
            .Select(r => r with { Balance = 0 })
            .FirstOrDefault();

        return record is null ? record : _repository.Update(record);
    }
}
