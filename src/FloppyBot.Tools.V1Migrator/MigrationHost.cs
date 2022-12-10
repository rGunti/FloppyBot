using System.Collections.Immutable;
using FloppyBot.Tools.V1Migrator.Migrations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Tools.V1Migrator;

public class MigrationHost : BackgroundService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger<MigrationHost> _logger;
    private readonly IImmutableList<IMigration> _migrations;

    public MigrationHost(
        ILogger<MigrationHost> logger,
        IEnumerable<IMigration> migrations,
        IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _migrations = migrations
            .OrderBy(m => m.Order)
            .ThenBy(m => m.GetType().FullName)
            .ToImmutableArray();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting migrations ...");
        try
        {
            foreach (IMigration migration in _migrations)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Cancellation requested, aborting ...");
                    break;
                }

                if (!migration.CanExecute())
                {
                    _logger.LogDebug("Skipping migration {MigrationOrder} {Migration}",
                        migration.Order,
                        migration.GetType().Name);
                    continue;
                }

                _logger.LogInformation(
                    "Executing migration {MigrationOrder} {Migration}",
                    migration.Order,
                    migration.GetType().Name);
                migration.Execute();
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Execution failed due to an exception!");
        }

        _appLifetime.StopApplication();
        return Task.CompletedTask;
    }
}


