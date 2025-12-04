using FloppyBot.Base.Clock;
using FloppyBot.Base.Extensions;
using FloppyBot.TwitchApi.Exceptions;
using FloppyBot.TwitchApi.Storage;
using FloppyBot.TwitchApi.Storage.Entities;
using Microsoft.Extensions.Logging;
using TwitchLib.Api.Auth;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Interfaces;

namespace FloppyBot.TwitchApi;

public class TwitchAuthenticator
{
    private readonly ILogger<TwitchAuthenticator> _logger;
    private readonly ITwitchAccessCredentialsService _credentialsService;
    private readonly ITwitchAccessCredentialInitiationService _initiationService;
    private readonly TwitchAuthenticationConfiguration _twitchConfiguration;
    private readonly ITwitchAPI _twitchApi;
    private readonly ITimeProvider _timeProvider;

    public TwitchAuthenticator(
        ILogger<TwitchAuthenticator> logger,
        ITwitchAccessCredentialsService credentialsService,
        ITwitchAccessCredentialInitiationService initiationService,
        TwitchAuthenticationConfiguration twitchConfiguration,
        ITwitchAPI twitchApi,
        ITimeProvider timeProvider
    )
    {
        _logger = logger;
        _credentialsService = credentialsService;
        _initiationService = initiationService;
        _twitchConfiguration = twitchConfiguration;
        _twitchApi = twitchApi;
        _timeProvider = timeProvider;
    }

    public string InitiateNewSession(string username, string channel)
    {
        _logger.LogDebug("Initiating new session for {Username} and {Channel}", username, channel);
        var initSession = _initiationService.GetOrCreateFor(
            username,
            channel,
            _twitchConfiguration.Scopes
        );
        var query = new Dictionary<string, string>
        {
            { "client_id", _twitchConfiguration.ClientId },
            { "response_type", "code" },
            { "redirect_uri", _twitchConfiguration.RedirectUrl },
            { "scope", _twitchConfiguration.Scopes.Join(" ") },
            { "state", initSession.Id },
            { "force_verify", "true" },
        };

        return $"https://id.twitch.tv/oauth2/authorize?{query.ToQueryString()}";
    }

    public string? GetChannelForSession(string sessionId)
    {
        return _initiationService
            .GetForSessionId(sessionId)
            .Select(i => i.ForChannel)
            .FirstOrDefault();
    }

    public bool HasLinkForChannel(string channel)
    {
        return _credentialsService.GetAccessCredentialsFor(channel).HasValue;
    }

    public async Task ConfirmSession(string username, string channel, string sessionId, string code)
    {
        // Check if we have a session open (throw otherwise)
        var initSession = _initiationService
            .GetFor(username, channel, _twitchConfiguration.Scopes)
            .Where(session => session.Id == sessionId)
            .OrThrow(TwitchAuthenticationException.NoSession);

        // Terminate the init session
        _initiationService.Delete(initSession);

        // Attempt to exchange the code for an access token
        _logger.LogDebug("Confirming session for {Username} and {Channel}", username, channel);
        AuthCodeResponse authCodeResponse;
        try
        {
            authCodeResponse = await _twitchApi.Auth.GetAccessTokenFromCodeAsync(
                code: code,
                clientId: _twitchConfiguration.ClientId,
                clientSecret: _twitchConfiguration.Secret,
                redirectUri: _twitchConfiguration.RedirectUrl
            );
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning(
                ex,
                "Confirming session for {Username} and {Channel} failed",
                username,
                channel
            );
            throw TwitchAuthenticationException.FailedToAcquireAccessToken(ex);
        }

        // Store the acquired credentials in the database
        var credentials = new TwitchAccessCredentials(
            channel,
            channel,
            authCodeResponse.AccessToken,
            authCodeResponse.RefreshToken,
            initSession.WithScopes,
            _timeProvider.GetCurrentUtcTime().Add(TimeSpan.FromSeconds(authCodeResponse.ExpiresIn))
        );
        _credentialsService.StoreAccessCredentials(credentials);
    }

    public async Task<TwitchCredentialValidation> ValidateCredentials(string channel)
    {
        _logger.LogDebug("Validating credentials for channel {Channel}", channel);

        var credentials = _credentialsService.GetAccessCredentialsFor(channel);
        if (!credentials.HasValue)
        {
            _logger.LogInformation("No credentials found for channel {Channel}", channel);
            return TwitchCredentialValidation.Missing;
        }

        var fullCredentials = credentials.Value;
        if (fullCredentials.ExpiresOn <= _timeProvider.GetCurrentUtcTime())
        {
            _logger.LogWarning("Credentials expired for channel {Channel}", channel);
            return TwitchCredentialValidation.Expired;
        }

        var result = await _twitchApi.Auth.ValidateAccessTokenAsync(fullCredentials.AccessToken);
        if (result is null)
        {
            _logger.LogInformation("Credentials invalid for channel {Channel}", channel);
            return TwitchCredentialValidation.Invalid;
        }

        _logger.LogInformation(
            "Credentials verified for channel {Channel}: {@ValidationResult}",
            channel,
            result
        );
        return TwitchCredentialValidation.Valid;
    }

    public async Task RefreshCredentials(string channel)
    {
        _logger.LogDebug("Refreshing credentials for channel {Channel}", channel);
        var credentials = _credentialsService
            .GetAccessCredentialsFor(channel)
            .OrThrow(() => TwitchAuthenticationException.NoCredentialsForChannel(channel));

        RefreshResponse refreshResponse;
        try
        {
            refreshResponse = await _twitchApi.Auth.RefreshAuthTokenAsync(
                refreshToken: credentials.RefreshToken,
                clientId: _twitchConfiguration.ClientId,
                clientSecret: _twitchConfiguration.Secret
            );
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning(ex, "Refreshing credentials for channel {Channel} failed", channel);
            throw TwitchAuthenticationException.FailedToRefreshAccessToken(ex);
        }

        _logger.LogDebug("Updated credentials for channel {Channel}", channel);
        var updatedCredentials = credentials with
        {
            AccessToken = refreshResponse.AccessToken,
            RefreshToken = refreshResponse.RefreshToken,
            ExpiresOn = _timeProvider
                .GetCurrentUtcTime()
                .Add(TimeSpan.FromSeconds(refreshResponse.ExpiresIn)),
        };
        _credentialsService.StoreAccessCredentials(updatedCredentials);
    }

    public void RevokeCredentials(string channel)
    {
        _logger.LogInformation("Revoking credentials for channel {Channel}", channel);
        _credentialsService.DeleteAccessCredentials(channel);
    }
}

public record TwitchAuthenticationConfiguration(
    string ClientId,
    string Secret,
    string RedirectUrl,
    string[] Scopes
)
{
    [Obsolete("This constructor is only present for configuration purposes and should not be used")]
    // ReSharper disable once UnusedMember.Global
    public TwitchAuthenticationConfiguration()
        : this(null!, null!, null!, []) { }
}
