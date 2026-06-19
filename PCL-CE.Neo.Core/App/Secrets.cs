using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace PCL_CE.Neo.Core.App;

public static class Secrets
{
    private static readonly ConcurrentDictionary<string, string> _secrets = new();
    private static byte[]? _encryptionKey;

    public static void Initialize(byte[] key)
    {
        _encryptionKey = key;
    }

    public static void Set(string key, string value)
    {
        if (_encryptionKey != null)
        {
            value = Encrypt(value);
        }
        _secrets[key] = value;
    }

    public static string? Get(string key)
    {
        if (_secrets.TryGetValue(key, out var value))
        {
            if (_encryptionKey != null)
            {
                try
                {
                    return Decrypt(value);
                }
                catch
                {
                    return null;
                }
            }
            return value;
        }
        return null;
    }

    public static bool Remove(string key)
    {
        return _secrets.TryRemove(key, out _);
    }

    public static bool Contains(string key)
    {
        return _secrets.ContainsKey(key);
    }

    public static void Clear()
    {
        _secrets.Clear();
    }

    private static string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var sw = new StreamWriter(cs);
        sw.Write(plainText);

        var iv = Convert.ToBase64String(aes.IV);
        var cipherText = Convert.ToBase64String(ms.ToArray());
        return $"{iv}:{cipherText}";
    }

    private static string Decrypt(string encryptedText)
    {
        var parts = encryptedText.Split(':');
        if (parts.Length != 2)
            throw new FormatException("Invalid encrypted format");

        var iv = Convert.FromBase64String(parts[0]);
        var cipherText = Convert.FromBase64String(parts[1]);

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.IV = iv;

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipherText);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}