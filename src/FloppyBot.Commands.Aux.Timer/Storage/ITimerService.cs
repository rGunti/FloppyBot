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
}

