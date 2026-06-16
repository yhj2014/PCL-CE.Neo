namespace PCL_CE.Neo.Core.Utils.Encryption;

public interface IEncryptionProvider
{
    byte[] Encrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key);
    byte[] Decrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key);
    bool IsSupported { get; }
}