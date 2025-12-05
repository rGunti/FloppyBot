using FloppyBot.Base.Clock;
using FloppyBot.Base.Cron;
using FloppyBot.Base.Cron.Attributes;
using FloppyBot.TwitchApi.Exceptions;
using FloppyBot.TwitchApi.Storage;
using FloppyBot.TwitchApi.Storage.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.TwitchApi;

#if DEBUG
[CronInterval(Milliseconds = 15_000, RunOnStartup = true)] // Every 15 seconds
#else
[CronInterval(Milliseconds = 30 * 60_000, RunOnStartup = true)] // Every 30 minutes
#endif
public class TwitchCredentialMonitor : ICronJob
{
    private readonly ILogger<TwitchCredentialMonitor> _logger;
    private readonly ITwitchAccessCredentialsService _credentialsService;
    private readonly TwitchAuthenticator _authenticator;
    private readonly ITimeProvider _timeProvider;

    public TwitchCredentialMonitor(
        ILogger<TwitchCredentialMonitor> logger,
        ITwitchAccessCredentialsService credentialsService,
        TwitchAuthenticator authenticator,
        ITimeProvider timeProvider
    )
    {
        _logger = logger;
        _credentialsService = credentialsService;
        _authenticator = authenticator;
        _timeProvider = timeProvider;
    }

    public void Run()
    {
        _logger.LogDebug("Starting credential monitor job");
        foreach (var channel in _credentialsService.GetAllKnownCredentials())
        {
            _logger.LogTrace("Checking credentials for channel {Channel}", channel);
            ProcessChannelAsync(channel).GetAwaiter().GetResult();
        }
    }

    private async Task ProcessChannelAsync(string channel)
    {
        (await _authenticator.ValidateCredentials(channel)).Deconstruct(
            out var verdict,
            out var expiresOn
        );

        var needsRenewal = verdict switch
        {
            TwitchCredentialValidation.Expired => true,
            TwitchCredentialValidation.Invalid => true,
            TwitchCredentialValidation.Valid => expiresOn!
                <= _timeProvider.GetCurrentUtcTime().AddMinutes(60),
            _ => false,
        };

        _logger.LogTrace(
            "Credentials validation completed for channel {Channel} with verdict {Verdict} and expiry {Expiry}",
            channel,
            verdict,
            expiresOn
        );
        if (needsRenewal)
        {
            _logger.LogInformation(
                "Credentials expired for channel {Channel}, attempting to refresh",
                channel
            );
            try
            {
                await _authenticator.RefreshCredentials(channel);
            }
            catch (TwitchAuthenticationException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to refresh credentials for channel {Channel}",
                    channel
                );
            }
        }
    }
}
