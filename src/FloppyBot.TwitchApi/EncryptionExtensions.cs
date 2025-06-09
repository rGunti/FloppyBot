using FloppyBot.Base.Encryption;
using FloppyBot.TwitchApi.Storage.Entities;

namespace FloppyBot.TwitchApi;

internal static class EncryptionExtensions
{
    internal static async Task<TwitchAccessCredentials> Encrypt(
        this IEncryptionShim encryptionShim,
        TwitchAccessCredentials credentials
    )
    {
        return credentials with
        {
            AccessToken = await encryptionShim.Encrypt(credentials.AccessToken),
            RefreshToken = await encryptionShim.Encrypt(credentials.RefreshToken),
        };
    }

    internal static async Task<TwitchAccessCredentials> Decrypt(
        this IEncryptionShim encryptionShim,
        TwitchAccessCredentials credentials
    )
    {
        return credentials with
        {
            AccessToken = await encryptionShim.Decrypt(credentials.AccessToken),
            RefreshToken = await encryptionShim.Decrypt(credentials.RefreshToken),
        };
    }

    internal static TwitchAccessCredentials EncryptSync(
        this IEncryptionShim encryptionShim,
        TwitchAccessCredentials credentials
    )
    {
        return encryptionShim.Encrypt(credentials).GetAwaiter().GetResult();
    }

    internal static TwitchAccessCredentials DecryptSync(
        this IEncryptionShim encryptionShim,
        TwitchAccessCredentials credentials
    )
    {
        return encryptionShim.Decrypt(credentials).GetAwaiter().GetResult();
    }
}
