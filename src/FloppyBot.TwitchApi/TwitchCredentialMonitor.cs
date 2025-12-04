using FloppyBot.Base.Cron;
using FloppyBot.Base.Cron.Attributes;
using FloppyBot.TwitchApi.Exceptions;
using FloppyBot.TwitchApi.Storage;
using FloppyBot.TwitchApi.Storage.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.TwitchApi;

[CronInterval(Milliseconds = 30 * 60_000)] // Every 30 minutes
public class TwitchCredentialMonitor : ICronJob
{
    private readonly ILogger<TwitchCredentialMonitor> _logger;
    private readonly ITwitchAccessCredentialsService _credentialsService;
    private readonly TwitchAuthenticator _authenticator;

    public TwitchCredentialMonitor(
        ILogger<TwitchCredentialMonitor> logger,
        ITwitchAccessCredentialsService credentialsService,
        TwitchAuthenticator authenticator
    )
    {
        _logger = logger;
        _credentialsService = credentialsService;
        _authenticator = authenticator;
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
        var validationResult = await _authenticator.ValidateCredentials(channel);
        switch (validationResult)
        {
            case TwitchCredentialValidation.Valid:
                _logger.LogInformation("Credentials validated for channel {Channel}", channel);
                return;
            case TwitchCredentialValidation.Expired or TwitchCredentialValidation.Invalid:
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

                break;
        }
    }
}
