namespace FloppyBot.Base.Encryption;

/// <summary>
/// An interface that encodes / decodes encrypted text
/// </summary>
public interface IEncryptionShim
{
    Task<string> Encrypt(string plainText);
    Task<string> Decrypt(string encryptedText);

    string EncryptSync(string plainText)
    {
        return Encrypt(plainText).GetAwaiter().GetResult();
    }

    string DecryptSync(string encryptedText)
    {
        return Decrypt(encryptedText).GetAwaiter().GetResult();
    }
}
