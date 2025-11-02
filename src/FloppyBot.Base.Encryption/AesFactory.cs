using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace FloppyBot.Base.Encryption;

public sealed class AesFactory
{
    private readonly IConfiguration _configuration;

    public AesFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public ICryptoTransform CreateEncryptor()
    {
        return Create().CreateEncryptor();
    }

    public ICryptoTransform CreateDecryptor()
    {
        return Create().CreateDecryptor();
    }

    private Aes Create()
    {
        var aes = Aes.Create();
        aes.Key = GetKey();
        aes.IV = GetIv();
        return aes;
    }

    private byte[] GetKey() => GetBytes("Aes:Key");

    private byte[] GetIv() => GetBytes("Aes:Iv");

    private byte[] GetBytes(string configKey)
    {
        return Convert.FromBase64String(
            _configuration[configKey] ?? throw new ArgumentException("ConfigKey is missing")
        );
    }
}
