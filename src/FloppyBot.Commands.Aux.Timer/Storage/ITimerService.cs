using System.Collections.Immutable;
using FloppyBot.Base.Clock;
using FloppyBot.Base.Storage;
using FloppyBot.Commands.Aux.Timer.Storage.Entities;

namespace FloppyBot.Commands.Aux.Timer.Storage;

public interface ITimerService
{
    void CreateTimer(
        string sourceMessage,
        string author,
        TimeSpan timerLength,
        string timerMessage);

    IEnumerable<TimerRecord> GetExpiredTimers(bool delete);
    void DeleteTimers(IEnumerable<TimerRecord> timers);
    void DeleteTimer(TimerRecord timer);
}

public class TimerService : ITimerService
{
    private readonly IRepository<TimerRecord> _repository;
    private readonly ITimeProvider _timeProvider;

    public TimerService(IRepositoryFactory repositoryFactory, ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _repository = repositoryFactory.GetRepository<TimerRecord>();
    }

    public void CreateTimer(string sourceMessage, string author, TimeSpan timerLength, string timerMessage)
    {
        DateTimeOffset now = _timeProvider.GetCurrentUtcTime();
        var timer = new TimerRecord(
            null!,
            sourceMessage,
            timerMessage,
            now + timerLength,
            now,
            author);
        _repository.Insert(timer);
    }

    public IEnumerable<TimerRecord> GetExpiredTimers(bool delete)
    {
        DateTimeOffset now = _timeProvider.GetCurrentUtcTime();
        ImmutableArray<TimerRecord> timers = _repository.GetAll()
            .Where(t => t.TargetTime <= now)
            .ToImmutableArray();

        if (timers.Any() && delete)
        {
            DeleteTimers(timers);
        }

        return timers;
    }

    public void DeleteTimers(IEnumerable<TimerRecord> timers)
    {
        _repository.Delete(timers);
    }

    public void DeleteTimer(TimerRecord timer)
    {
        _repository.Delete(timer);
    }
}


