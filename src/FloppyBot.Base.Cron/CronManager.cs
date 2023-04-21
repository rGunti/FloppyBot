using System.Collections.Immutable;
using System.Reflection;
using FloppyBot.Base.Cron.Attributes;
using FluentScheduler;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Base.Cron;

public interface ICronManager : IDisposable
{
    void Launch();
}

internal record CronJobInfo(
    ICronJob Implementation,
    int Interval,
    bool RunOnStartup,
    Type ThreadType
);

public class CronManager : ICronManager
{
    private readonly IImmutableList<CronJobInfo> _cronJobs;
    private readonly ILogger<CronManager> _logger;

    public CronManager(ILogger<CronManager> logger, IEnumerable<ICronJob> cronThreads)
    {
        logger.LogDebug($"Creating {nameof(CronManager)} ...");
        _logger = logger;
        _cronJobs = cronThreads.Select(CreateCronJob).ToImmutableList();
    }

    public void Launch()
    {
        _logger.LogInformation("Starting cron job threads ...");
        _logger.LogTrace("Initializing Job Manager ...");
        JobManager.Initialize();

        _logger.LogDebug("Registering cron jobs ...");
        foreach (var cronJob in _cronJobs)
        {
            _logger.LogTrace("Adding {@CronJobType}", cronJob);
            JobManager.AddJob(
                () => cronJob.Implementation.Run(),
                s =>
                {
                    if (cronJob.RunOnStartup)
                    {
                        s.ToRunNow().AndEvery(cronJob.Interval).Milliseconds();
                    }
                    else
                    {
                        s.ToRunEvery(cronJob.Interval).Milliseconds();
                    }
                }
            );
        }

        JobManager.Start();
    }

    public void Dispose()
    {
        _logger.LogInformation(
            "Stopping {CronJobCount} cron job threads to dispose ...",
            _cronJobs.Count
        );
        JobManager.StopAndBlock();
    }

    private static CronJobInfo CreateCronJob(ICronJob cronJob)
    {
        var type = cronJob.GetType();
        if (!type.GetCustomAttributes<CronIntervalAttribute>().Any())
        {
            throw new ArgumentException(
                $"The provided type {type} does not have a {nameof(CronIntervalAttribute)} attached.",
                nameof(type)
            );
        }

        var settings = type.GetCustomAttributes<CronIntervalAttribute>().First();

        return new CronJobInfo(cronJob, settings.Milliseconds, settings.RunOnStartup, type);
    }
}
