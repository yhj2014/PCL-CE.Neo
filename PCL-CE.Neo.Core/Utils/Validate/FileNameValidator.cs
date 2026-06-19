using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class FileNameValidator
{
    public bool UseMinecraftCharCheck { get; set; } = true;
    public bool IgnoreCase { get; set; } = true;
    public string? ParentFolder { get; set; }
    public bool RequireParentFolderExists { get; set; } = true;

    private bool? _isParentFolderExists;

    public FileNameValidator(
        string? parentFolder = null,
        bool ignoreCase = true,
        bool useMinecraftCharCheck = true,
        bool requireParentFolderExists = true)
    {
        ParentFolder = parentFolder;
        IgnoreCase = ignoreCase;
        UseMinecraftCharCheck = useMinecraftCharCheck;
        RequireParentFolderExists = requireParentFolderExists;
    }

    public FileNameValidator() : this(null)
    {
    }

    public bool Validate(string fileName)
    {
        return ValidateAndGetError(fileName) == null;
    }

    public string? ValidateAndGetError(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "输入内容不能为空！";

        if (fileName.StartsWith(' '))
            return "文件名不能以空格开头！";

        if (fileName.EndsWith(' '))
            return "文件名不能以空格结尾！";

        if (fileName.EndsWith('.'))
            return "文件名不能以小数点结尾！";

        var invalidChar = CheckInvalidStrings(fileName, UseMinecraftCharCheck ? ["!;"] : []);
        if (invalidChar != null)
            return $"文件名不可包含 {invalidChar} 字符！";

        var reservedWord = CheckReservedWord(fileName);
        if (reservedWord != null)
            return $"文件名不可为 {reservedWord}！";

        if (Regex.IsMatch(fileName, RegexPatterns.Ntfs83FileName))
            return "文件名不能包含这一特殊格式！";

        if (ParentFolder != null)
        {
            var dirInfo = new DirectoryInfo(ParentFolder);
            if (dirInfo.Exists)
            {
                var exists = dirInfo.EnumerateFiles()
                    .Select(f => f.Name)
                    .Contains(fileName, IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
                if (exists)
                    return "不可与现有文件重名！";
            }
            else
            {
                _isParentFolderExists = false;
                if (RequireParentFolderExists)
                    return $"父文件夹不存在：{ParentFolder}";
            }
        }

        return null;
    }

    private static string? CheckInvalidStrings(string fileName, string[] invalidStrings)
    {
        foreach (var invalid in invalidStrings)
        {
            if (fileName.Contains(invalid))
                return invalid;
        }
        return null;
    }

    private static string? CheckReservedWord(string fileName)
    {
        var reservedWords = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
        var upperName = fileName.ToUpperInvariant();
        foreach (var word in reservedWords)
        {
            if (upperName == word)
                return word;
        }
        return null;
    }
}