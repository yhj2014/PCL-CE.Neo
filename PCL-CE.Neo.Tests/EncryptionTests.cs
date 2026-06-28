using System;
using PCL_CE.Neo.Core.Utils.Encryption;
using Xunit;

namespace PCL_CE.Neo.Tests;

public class EncryptionTests
{
    [Fact]
    public void AesGcmProvider_IsSupported_ReturnsTrue()
    {
        Assert.True(AesGcmProvider.Instance.IsSupported);
    }

    [Fact]
    public void AesGcmProvider_EncryptDecrypt_RoundTrips()
    {
        var provider = AesGcmProvider.Instance;
        var key = new byte[32];
        new Random(42).NextBytes(key);
        var originalData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        var encrypted = provider.Encrypt(originalData, key);
        var decrypted = provider.Decrypt(encrypted, key);

        Assert.NotEqual(originalData, encrypted);
        Assert.Equal(originalData, decrypted);
    }

    [Fact]
    public void AesGcmProvider_Encrypt_ProducesDifferentOutputEachTime()
    {
        var provider = AesGcmProvider.Instance;
        var key = new byte[32];
        new Random(42).NextBytes(key);
        var data = new byte[] { 1, 2, 3, 4, 5 };

        var encrypted1 = provider.Encrypt(data, key);
        var encrypted2 = provider.Encrypt(data, key);

        Assert.NotEqual(encrypted1, encrypted2);
    }

    [Fact]
    public void ChaCha20Poly1305Provider_IsSupported_ReturnsTrue()
    {
        Assert.True(ChaCha20Poly1305Provider.Instance.IsSupported);
    }

    [Fact]
    public void ChaCha20Poly1305Provider_EncryptDecrypt_RoundTrips()
    {
        var provider = ChaCha20Poly1305Provider.Instance;
        var key = new byte[32];
        new Random(42).NextBytes(key);
        var originalData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        var encrypted = provider.Encrypt(originalData, key);
        var decrypted = provider.Decrypt(encrypted, key);

        Assert.NotEqual(originalData, encrypted);
        Assert.Equal(originalData, decrypted);
    }

    [Fact]
    public void ChaCha20SoftwareProvider_IsSupported_ReturnsTrue()
    {
        Assert.True(ChaCha20SoftwareProvider.Instance.IsSupported);
    }

    [Fact]
    public void ChaCha20SoftwareProvider_EncryptDecrypt_RoundTrips()
    {
        var provider = ChaCha20SoftwareProvider.Instance;
        var key = new byte[32];
        new Random(42).NextBytes(key);
        var originalData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

        var encrypted = provider.Encrypt(originalData, key);
        var decrypted = provider.Decrypt(encrypted, key);

        Assert.NotEqual(originalData, encrypted);
        Assert.Equal(originalData, decrypted);
    }

    [Fact]
    public void ChaCha20SoftwareProvider_ProducesDifferentOutputEachTime()
    {
        var provider = ChaCha20SoftwareProvider.Instance;
        var key = new byte[32];
        new Random(42).NextBytes(key);
        var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

        var encrypted1 = provider.Encrypt(data, key);
        var encrypted2 = provider.Encrypt(data, key);

        Assert.NotEqual(encrypted1, encrypted2);
    }

    [Fact]
    public void ChaCha20SoftwareProvider_Decrypt_ThrowsOnInvalidData()
    {
        var provider = ChaCha20SoftwareProvider.Instance;
        var key = new byte[32];
        new Random(42).NextBytes(key);
        var invalidData = new byte[] { 1, 2, 3 };

        Assert.Throws<ArgumentException>(() => provider.Decrypt(invalidData, key));
    }
}
