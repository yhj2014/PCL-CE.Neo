using System.Collections.Generic;
using System.IO;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class FolderPathValidator : FileSystemValidator
{
    public bool UseMinecraftCharCheck { get; set; }

    public FolderPathValidator(bool useMinecraftCharCheck)
    {
        UseMinecraftCharCheck = useMinecraftCharCheck;
    }

    public FolderPathValidator() : this(true)
    {
    }

    public override ValidationResult Validate(string value)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(value))
        {
            errors.Add("输入内容不能为空！");
            return ValidationResult.Failure(errors);
        }

        if (value.EndsWith(' '))
            errors.Add("文件夹名不能以空格结尾！");

        if (value.EndsWith('.'))
            errors.Add("文件夹名不能以小数点结尾！");

        var subPaths = GetSubPaths(value);
        foreach (var subPath in subPaths)
        {
            if (string.IsNullOrWhiteSpace(subPath))
            {
                errors.Add("文件夹路径存在错误！");
                continue;
            }

            if (subPath.StartsWith(' '))
                errors.Add("文件夹名不能以空格开头！");

            if (subPath.EndsWith(' '))
                errors.Add("文件夹名不能以空格结尾！");

            if (subPath.EndsWith('.'))
                errors.Add("文件夹名不能以小数点结尾！");

            var invalidChar = CheckInvalidStrings(subPath, UseMinecraftCharCheck ? ["!;"] : []);
            if (invalidChar != null)
            {
                errors.Add($"文件夹名不可包含 {invalidChar} 字符！");
            }

            var reservedWord = CheckReservedWord(subPath, []);
            if (reservedWord != null)
            {
                errors.Add($"文件夹名不可为 {reservedWord}！");
            }

            if (RegexPatterns.Ntfs83FileName.IsMatch(subPath))
                errors.Add("文件夹名不能包含这一特殊格式！");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    private static string[] GetSubPaths(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return [];
        }

        var fullPath = new DirectoryInfo(path).FullName;
        return fullPath[Path.GetPathRoot(fullPath)!.Length..]
            .TrimEnd(Path.DirectorySeparatorChar)
            .Split(Path.DirectorySeparatorChar, System.StringSplitOptions.RemoveEmptyEntries);
    }
}