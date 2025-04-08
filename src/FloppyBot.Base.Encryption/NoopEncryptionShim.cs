namespace FloppyBot.Base.Encryption;

/// <summary>
/// An implementation of <see cref="IEncryptionShim"/> that does nothing.
/// Useful for testing.
/// </summary>
public class NoopEncryptionShim : IEncryptionShim
{
    public Task<string> Encrypt(string plainText)
    {
        return Task.FromResult(plainText);
    }

    public Task<string> Decrypt(string encryptedText)
    {
        return Task.FromResult(encryptedText);
    }
}
