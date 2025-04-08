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
        var initSession = _initiationService.GetOrCreateFor(username, channel);
        var query = new Dictionary<string, string>
        {
            { "client_id", _twitchConfiguration.ClientId },
            { "response_type", "code" },
            { "redirect_uri", _twitchConfiguration.RedirectUrl },
            { "scope", _twitchConfiguration.Scopes.Join(" ") },
            { "state", initSession.Id },
        };

        return $"https://id.twitch.tv/oauth2/authorize?{query.ToQueryString()}";
    }

    public async Task ConfirmSession(string username, string channel, string sessionId, string code)
    {
        // Check if we have a session open (throw otherwise)
        var initSession = _initiationService
            .GetFor(username, channel)
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
                clientSecret: _twitchConfiguration.ClientSecret,
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
            null!,
            channel,
            authCodeResponse.AccessToken,
            authCodeResponse.RefreshToken,
            _timeProvider.GetCurrentUtcTime().Add(TimeSpan.FromSeconds(authCodeResponse.ExpiresIn))
        );
        _credentialsService.StoreAccessCredentials(credentials);
    }
}

public record TwitchAuthenticationConfiguration(
    string ClientId,
    string ClientSecret,
    string RedirectUrl,
    string[] Scopes
)
{
    [Obsolete("This constructor is only present for configuration purposes and should not be used")]
    // ReSharper disable once UnusedMember.Global
    public TwitchAuthenticationConfiguration()
        : this(null!, null!, null!, []) { }
}
