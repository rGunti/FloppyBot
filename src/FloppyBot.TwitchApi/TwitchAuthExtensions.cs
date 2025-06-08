using FloppyBot.Base.Encryption;
using FloppyBot.TwitchApi.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.TwitchApi;

public static class TwitchAuthExtensions
{
    public static IServiceCollection AddTwitchAuth(this IServiceCollection services)
    {
        return services
            .AddSingleton<TwitchAuthenticator>()
            .AddSingleton<
                ITwitchAccessCredentialInitiationService,
                TwitchAccessCredentialInitiationService
            >()
            .AddSingleton<ITwitchAccessCredentialsService, TwitchAccessCredentialsService>()
            .AddSingleton<IEncryptionShim, Base64EncodingShim>()
            .AddSingleton<TwitchAuthenticationConfiguration>(s =>
            {
                var config = s.GetRequiredService<IConfiguration>();
                return config.GetSection("Twitch").Get<TwitchAuthenticationConfiguration>()
                    ?? throw new Exception("Twitch configuration not found");
            });
    }
}
