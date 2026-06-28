using System;
using System.IO;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Utils;
using Xunit;

namespace PCL_CE.Neo.Tests;

public class VarIntTests
{
    [Fact]
    public void EncodeDecode_RoundTrips_ForZero()
    {
        var encoded = VarIntHelper.Encode(0UL);
        var decoded = VarIntHelper.Decode(encoded, out var readLength);
        
        Assert.Equal(0UL, decoded);
        Assert.Equal(1, readLength);
    }

    [Fact]
    public void EncodeDecode_RoundTrips_For127()
    {
        var encoded = VarIntHelper.Encode(127UL);
        var decoded = VarIntHelper.Decode(encoded, out var readLength);
        
        Assert.Equal(127UL, decoded);
        Assert.Single(encoded);
    }

    [Fact]
    public void EncodeDecode_RoundTrips_For128()
    {
        var encoded = VarIntHelper.Encode(128UL);
        var decoded = VarIntHelper.Decode(encoded, out var readLength);
        
        Assert.Equal(128UL, decoded);
        Assert.Equal(2, readLength);
    }

    [Fact]
    public void EncodeDecode_RoundTrips_ForLargeNumber()
    {
        var original = 3000000UL;
        var encoded = VarIntHelper.Encode(original);
        var decoded = VarIntHelper.Decode(encoded, out var readLength);
        
        Assert.Equal(original, decoded);
    }

    [Fact]
    public void Encode_UInt_RoundTrips()
    {
        var original = 1000000U;
        var encoded = VarIntHelper.Encode(original);
        var decoded = VarIntHelper.Decode(encoded, out var readLength);
        
        Assert.Equal(original, decoded);
    }

    [Fact]
    public async Task ReadFromStreamAsync_RoundTrips()
    {
        using var ms = new MemoryStream();
        var original = 12345UL;
        var encoded = VarIntHelper.Encode(original);
        await ms.WriteAsync(encoded);
        ms.Position = 0;

        var decoded = await VarIntHelper.ReadFromStreamAsync(ms);
        
        Assert.Equal(original, decoded);
    }

    [Fact]
    public void DecodeZigZag_EncodesAndDecodesCorrectly()
    {
        Assert.Equal(0L, VarIntHelper.DecodeZigZag(VarIntHelper.EncodeZigZag(0)));
        Assert.Equal(1L, VarIntHelper.DecodeZigZag(VarIntHelper.EncodeZigZag(1)));
        Assert.Equal(-1L, VarIntHelper.DecodeZigZag(VarIntHelper.EncodeZigZag(ulong.MaxValue)));
        Assert.Equal(12345L, VarIntHelper.DecodeZigZag(VarIntHelper.EncodeZigZag(12345)));
    }
}
