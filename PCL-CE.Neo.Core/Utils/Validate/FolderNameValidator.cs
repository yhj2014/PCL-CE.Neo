using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class FolderNameValidator : FileSystemValidator
{
    public bool UseMinecraftCharCheck { get; set; }
    public bool IgnoreCase { get; set; }
    public bool IgnoreSameNameInParentFolder { get; set; }
    public string? ParentFolder { get; set; }

    public FolderNameValidator(
        string? parentFolder = null,
        bool useMinecraftCharCheck = true,
        bool ignoreCase = true,
        bool ignoreSameNameInParentFolder = true)
    {
        UseMinecraftCharCheck = useMinecraftCharCheck;
        IgnoreCase = ignoreCase;
        IgnoreSameNameInParentFolder = ignoreSameNameInParentFolder;
        ParentFolder = parentFolder;
    }

    public FolderNameValidator() : this(null)
    {
    }

    public override ValidationResult Validate(string value)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add("输入内容不能为空！");
        }
        else
        {
            if (value.StartsWith(' '))
                errors.Add("文件名不能以空格开头！");

            if (value.EndsWith(' '))
                errors.Add("文件名不能以空格结尾！");

            if (value.EndsWith('.'))
                errors.Add("文件名不能以小数点结尾！");

            var invalidChar = CheckInvalidStrings(value, UseMinecraftCharCheck ? ["!;"] : []);
            if (invalidChar != null)
            {
                errors.Add($"文件名不可包含 {invalidChar} 字符！");
            }

            var reservedWord = CheckReservedWord(value, []);
            if (reservedWord != null)
            {
                errors.Add($"文件名不可为 {reservedWord}！");
            }

            if (RegexPatterns.Ntfs83FileName.IsMatch(value))
                errors.Add("文件名不能包含这一特殊格式！");

            if (ParentFolder != null && !IgnoreSameNameInParentFolder)
            {
                var dirInfo = new DirectoryInfo(ParentFolder);
                if (dirInfo.Exists)
                {
                    var exists = dirInfo.EnumerateFiles().Any(f =>
                        string.Equals(f.Name, value,
                            IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
                    if (exists)
                        errors.Add("不可与现有文件夹重名！");
                }
            }
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }
}