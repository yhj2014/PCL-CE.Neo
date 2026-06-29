using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PCL_CE.Neo.Core.Utils;

public static class PEHeaderReader
{
    private const int PE_POINTER_OFFSET = 0x3C;
    private const uint PE_SIGNATURE = 0x00004550;

    public static PEStruct ReadPEHeader(string filePath)
    {
        var result = new PEStruct();

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            result.ErrorMessage = "文件不存在或路径无效";
            return result;
        }

        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (!_IsValidDosHeader(fs))
            {
                result.ErrorMessage = "无效的DOS头(MZ签名)";
                return result;
            }

            var peHeaderOffset = _GetPEOffset(fs);
            if (peHeaderOffset <= 0 || peHeaderOffset >= fs.Length - 24)
            {
                result.ErrorMessage = "无效的PE头偏移量";
                return result;
            }

            fs.Seek(peHeaderOffset, SeekOrigin.Begin);
            if (!_IsValidPESignature(fs))
            {
                result.ErrorMessage = "无效的PE签名";
                return result;
            }

            result = _ParseImageFileHeader(fs);
            result.IsValid = true;
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.ErrorMessage = $"读取失败: {ex.Message}";
        }
        return result;
    }

    private static bool _IsValidDosHeader(FileStream fs)
    {
        if (fs.Length < 2) return false;
        fs.Seek(0, SeekOrigin.Begin);
        return fs.ReadByte() == 'M' && fs.ReadByte() == 'Z';
    }

    private static long _GetPEOffset(FileStream fs)
    {
        fs.Seek(PE_POINTER_OFFSET, SeekOrigin.Begin);
        using var reader = new BinaryReader(fs, Encoding.Default, true);
        return reader.ReadInt32();
    }

    private static bool _IsValidPESignature(FileStream fs)
    {
        using var reader = new BinaryReader(fs, Encoding.Default, true);
        return reader.ReadUInt32() == PE_SIGNATURE;
    }

    private static PEStruct _ParseImageFileHeader(FileStream fs)
    {
        using var reader = new BinaryReader(fs, Encoding.Default, true);
        return new PEStruct
        {
            Machine = (MachineType)reader.ReadUInt16(),
            NumberOfSections = reader.ReadUInt16(),
            TimeDateStamp = reader.ReadUInt32(),
            PointerToSymbolTable = reader.ReadUInt32(),
            NumberOfSymbols = reader.ReadUInt32(),
            SizeOfOptionalHeader = reader.ReadUInt16(),
            Characteristics = reader.ReadUInt16()
        };
    }

    public static bool IsMachine64Bit(MachineType machine)
    {
        return new List<MachineType> { MachineType.IA64, MachineType.ARM64, MachineType.AMD64 }.Contains(machine);
    }
}

public enum MachineType : ushort
{
    Unknown = 0x0,
    I386 = 0x14C,
    IA64 = 0x200,
    AMD64 = 0x8664,
    ARM = 0x1C0,
    ARM64 = 0xAA64,
    ARMNT = 0x1C4,
    EFI_BYTECODE = 0xEBC,
    M32R = 0x9041,
    MIPS16 = 0x266,
    MIPSFPU = 0x366,
    MIPSFPU16 = 0x466,
    POWERPC = 0x1F0,
    POWERPCFP = 0x1F1,
    R4000 = 0x166,
    SH3 = 0x1A2,
    SH3DSP = 0x1A3,
    SH4 = 0x1A6,
    SH5 = 0x1A8,
    THUMB = 0x1C2,
    WCEMIPSV2 = 0x169,
}

[Serializable]
public struct PEStruct
{
    public MachineType Machine;
    public ushort NumberOfSections;
    public uint TimeDateStamp;
    public uint PointerToSymbolTable;
    public uint NumberOfSymbols;
    public ushort SizeOfOptionalHeader;
    public ushort Characteristics;
    public bool IsValid;
    public string ErrorMessage;
}