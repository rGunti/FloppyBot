using System.Text;

namespace FloppyBot.Base.Encryption;

/// <summary>
/// An implementation of <see cref="IEncryptionShim"/> that converts
/// the given text from and to Base64.
/// </summary>
/// <remarks>
/// This is not encryption!
/// </remarks>
public class Base64EncodingShim : IEncryptionShim
{
    public Task<string> Encrypt(string plainText)
    {
        var bytes = Convert.FromBase64String(plainText);
        return Task.FromResult(Convert.ToBase64String(bytes));
    }

    public Task<string> Decrypt(string encryptedText)
    {
        var bytes = Convert.FromBase64String(encryptedText);
        return Task.FromResult(Encoding.UTF8.GetString(bytes));
    }
}
