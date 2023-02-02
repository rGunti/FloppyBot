using FloppyBot.Base.Extensions;
using FloppyBot.Base.Storage;
using FloppyBot.Commands.Aux.Twitch.Storage.Entities;

namespace FloppyBot.Commands.Aux.Twitch.Storage;

public class TimerMessageConfigurationService : ITimerMessageConfigurationService
{
    private readonly IRepository<TimerMessageExecution> _executionRepo;
    private readonly IRepository<TimerMessageConfiguration> _timerMessageConfigRepo;

    public TimerMessageConfigurationService(IRepositoryFactory repositoryFactory)
    {
        _timerMessageConfigRepo = repositoryFactory.GetRepository<TimerMessageConfiguration>();
        _executionRepo = repositoryFactory.GetRepository<TimerMessageExecution>();
    }

    public IEnumerable<TimerMessageConfiguration> GetAllConfigs()
    {
        return _timerMessageConfigRepo.GetAll()
            // ReSharper disable once MergeIntoPattern
            .Where(c => c.Interval > 0 && c.Messages.Length > 0);
    }

    public NullableObject<TimerMessageConfiguration> GetConfigForChannel(string channelId)
    {
        return _timerMessageConfigRepo.GetById(channelId).Wrap();
    }

    public NullableObject<TimerMessageExecution> GetLastExecution(string channelId)
    {
        return _executionRepo.GetById(channelId).Wrap();
    }

    public void UpdateConfigurationForChannel(string channelId, TimerMessageConfiguration configuration)
    {
        TimerMessageConfiguration existingConfig = GetConfigForChannel(channelId)
            .FirstOrDefault(new TimerMessageConfiguration(
                string.Empty,
                Array.Empty<string>(),
                -1,
                0));

        existingConfig = existingConfig with
        {
            Messages = configuration.Messages,
            Interval = configuration.Interval,
            MinMessages = configuration.MinMessages,
        };
        if (existingConfig.Id == string.Empty)
        {
            _timerMessageConfigRepo.Insert(existingConfig.WithId(channelId));
        }
        else
        {
            _timerMessageConfigRepo.Update(existingConfig);
        }
    }

    public void UpdateLastExecutionTime(string channelId, DateTimeOffset executionTime, int messageIndex)
    {
        TimerMessageExecution lastExecution = GetLastExecution(channelId)
            .FirstOrDefault(new TimerMessageExecution(
                string.Empty,
                DateTimeOffset.MinValue,
                0));
        lastExecution = lastExecution with
        {
            LastExecutedAt = executionTime,
            MessageIndex = messageIndex,
        };

        if (lastExecution.Id == string.Empty)
        {
            _executionRepo.Insert(lastExecution.WithId(channelId));
        }
        else
        {
            _executionRepo.Update(lastExecution);
        }
    }
}

