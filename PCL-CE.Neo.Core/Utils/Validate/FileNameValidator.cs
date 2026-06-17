using PCL_CE.Neo.Core.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class FileNameValidator(
    string? parentFolder = null,
    bool ignoreCase = true,
    bool useMinecraftCharCheck = true,
    bool requireParentFolderExists = true) : IValidator<string>
{
    public bool UseMinecraftCharCheck { get; set; } = useMinecraftCharCheck;
    public bool IgnoreCase { get; set; } = ignoreCase;
    public string? ParentFolder { get; set; } = parentFolder;
    public bool RequireParentFolderExists { get; set; } = requireParentFolderExists;

    public ValidationResult Validate(string fileName)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(fileName))
        {
            errors.Add("输入内容不能为空！");
        }
        else
        {
            if (fileName.StartsWith(' '))
                errors.Add("文件名不能以空格开头！");

            if (fileName.EndsWith(' '))
                errors.Add("文件名不能以空格结尾！");

            if (fileName.EndsWith('.'))
                errors.Add("文件名不能以小数点结尾！");

            if (UseMinecraftCharCheck)
            {
                var invalidChars = new[] { '!', ';' };
                foreach (var ch in invalidChars)
                {
                    if (fileName.Contains(ch))
                    {
                        errors.Add($"文件名不可包含 '{ch}' 字符！");
                        break;
                    }
                }
            }

            if (RegexPatterns.Ntfs83FileName.IsMatch(fileName))
                errors.Add("文件名不能包含这一特殊格式！");

            if (ParentFolder != null)
            {
                var dirInfo = new DirectoryInfo(ParentFolder);
                if (dirInfo.Exists)
                {
                    var exists = dirInfo.EnumerateFiles().Any(f => 
                        string.Equals(f.Name, fileName, 
                            IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
                    if (exists)
                        errors.Add("不可与现有文件重名！");
                }
                else if (RequireParentFolderExists)
                {
                    errors.Add($"父文件夹不存在：{ParentFolder}");
                }
            }
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }
}