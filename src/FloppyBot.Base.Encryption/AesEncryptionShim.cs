using System.Text;

namespace FloppyBot.Base.Encryption;

public sealed class AesEncryptionShim : IEncryptionShim
{
    private readonly AesFactory _aesFactory;

    public AesEncryptionShim(AesFactory aesFactory)
    {
        _aesFactory = aesFactory;
    }

    public Task<string> Encrypt(string plainText)
    {
        using var crypto = _aesFactory.CreateEncryptor();
        var bytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = Convert.ToBase64String(crypto.TransformFinalBlock(bytes, 0, bytes.Length));
        return Task.FromResult(encrypted);
    }

    public Task<string> Decrypt(string encryptedText)
    {
        using var crypto = _aesFactory.CreateDecryptor();
        var bytes = Convert.FromBase64String(encryptedText);
        var decrypted = Encoding.UTF8.GetString(crypto.TransformFinalBlock(bytes, 0, bytes.Length));
        return Task.FromResult(decrypted);
    }
}
