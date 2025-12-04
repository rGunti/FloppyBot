using FloppyBot.Base.Cron;
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
            .AddSingleton<IEncryptionShim>(s =>
                GetEncryptionShim(s.GetRequiredService<IConfiguration>())
            )
            .AddSingleton<TwitchAuthenticationConfiguration>(s =>
            {
                var config = s.GetRequiredService<IConfiguration>();
                return config.GetSection("TwitchApi").Get<TwitchAuthenticationConfiguration>()
                    ?? throw new Exception("Twitch configuration not found");
            });
    }

    public static IServiceCollection AddTwitchCredentialMonitor(this IServiceCollection services)
    {
        return services.AddCronJob<TwitchCredentialMonitor>();
    }

    private static IEncryptionShim GetEncryptionShim(IConfiguration configuration)
    {
        var configSection = configuration.GetSection("Encryption");
        var type = configSection.GetValue<string>("Type", "Base64");

        return type switch
        {
            "None" or "Noop" => NoopEncryptionShim.Instance,
            "Base64" => Base64EncodingShim.Instance,
            "Aes" => new AesEncryptionShim(new AesFactory(configSection)),
            _ => throw new Exception($"Unknown encryption type {type}"),
        };
    }
}
