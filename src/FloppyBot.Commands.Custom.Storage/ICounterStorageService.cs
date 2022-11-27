using FloppyBot.Base.Extensions;
using FloppyBot.Base.Storage;
using FloppyBot.Commands.Custom.Storage.Entities.Internal;

namespace FloppyBot.Commands.Custom.Storage;

public interface ICounterStorageService
{
    int Peek(string commandId);
    int Next(string commandId);
    void Set(string commandId, int value);
    int Increase(string commandId, int increment);
}

public class CounterStorageService : ICounterStorageService
{
    private readonly IRepository<CounterEo> _repository;

    public CounterStorageService(IRepositoryFactory repositoryFactory)
    {
        _repository = repositoryFactory.GetRepository<CounterEo>();
    }

    public int Peek(string commandId)
    {
        return GetCounter(commandId)
            .Select(i => i.Value)
            .FirstOrDefault();
    }

    public int Next(string commandId)
    {
        NullableObject<CounterEo> wrappedCounter = GetCounter(commandId);
        if (!wrappedCounter.HasValue)
        {
            wrappedCounter = CreateCounter(commandId);
        }

        CounterEo counter = wrappedCounter.Value;
        return UpdateCounter(counter with
        {
            Value = counter.Value + 1
        });
    }

    public void Set(string commandId, int value)
    {
        NullableObject<CounterEo> counter = GetCounter(commandId);
        if (!counter.HasValue)
        {
            CreateCounter(commandId, value);
        }
        else
        {
            UpdateCounter(counter.Value with
            {
                Value = value
            });
        }
    }

    public int Increase(string commandId, int increment)
    {
        NullableObject<CounterEo> counter = GetCounter(commandId);
        bool needsInsert = !counter.HasValue;
        int newValue = counter
            .Select(c => c.Value)
            .FirstOrDefault() + increment;
        if (needsInsert)
        {
            CreateCounter(commandId, newValue);
        }
        else
        {
            UpdateCounter(counter.Value with
            {
                Value = newValue
            });
        }

        return newValue;
    }

    private NullableObject<CounterEo> GetCounter(string commandId)
    {
        return _repository.GetById(commandId).Wrap();
    }

    private NullableObject<CounterEo> CreateCounter(string commandId, int initialValue = 0)
    {
        return _repository.Insert(new CounterEo(commandId, initialValue)).Wrap();
    }

    private int UpdateCounter(CounterEo counter)
    {
        CounterEo updated = _repository.Update(counter);
        return updated.Value;
    }
}
