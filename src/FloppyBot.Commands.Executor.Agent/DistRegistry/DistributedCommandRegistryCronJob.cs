using FloppyBot.Base.Cron;
using FloppyBot.Base.Cron.Attributes;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Executor.Agent.DistRegistry;

[CronInterval(Milliseconds = 300_000, RunOnStartup = true)] // Run every 9 minutes instead, since records should expire after 10 minutes
public class DistributedCommandRegistryCronJob : ICronJob
{
    private readonly ILogger<DistributedCommandRegistryCronJob> _logger;
    private readonly DistributedCommandRegistryAdapter _registryAdapter;

    public DistributedCommandRegistryCronJob(
        ILogger<DistributedCommandRegistryCronJob> logger,
        DistributedCommandRegistryAdapter registryAdapter
    )
    {
        _logger = logger;
        _registryAdapter = registryAdapter;
    }

    public void Run()
    {
        _logger.LogTrace("Refreshing the distributed command registry ...");
        _registryAdapter.ScanAndStoreCommands();
    }
}
