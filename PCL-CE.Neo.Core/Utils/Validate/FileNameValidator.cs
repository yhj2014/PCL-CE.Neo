using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PCL.CE.Neo.Core.Utils.Validate;

public class FileNameValidator : IValidator<string>
{
    public bool UseMinecraftCharCheck { get; set; } = true;
    public bool IgnoreCase { get; set; } = true;
    public string? ParentFolder { get; set; }
    public bool RequireParentFolderExists { get; set; } = true;

    private bool? _isParentFolderExists;

    public ValidationResult Validate(string fileName)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(fileName))
        {
            errors.Add("输入内容不能为空！");
        }

        if (!errors.Any())
        {
            if (fileName.StartsWith(' '))
                errors.Add("文件名不能以空格开头！");

            if (fileName.EndsWith(' '))
                errors.Add("文件名不能以空格结尾！");

            if (fileName.EndsWith('.'))
                errors.Add("文件名不能以小数点结尾！");

            if (UseMinecraftCharCheck)
            {
                foreach (var invalidChar in new[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*', '!', ';' })
                {
                    if (fileName.Contains(invalidChar))
                    {
                        errors.Add($"文件名不可包含 {invalidChar} 字符！");
                        break;
                    }
                }
            }

            if (fileName.IsMatch(RegexPatterns.Ntfs83FileName))
                errors.Add("文件名不能包含这一特殊格式！");

            if (ParentFolder is not null)
            {
                var dirInfo = new DirectoryInfo(ParentFolder);
                if (dirInfo.Exists)
                {
                    if (dirInfo.EnumerateFiles().Any(f => 
                        string.Equals(f.Name, fileName, 
                            IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)))
                    {
                        errors.Add("不可与现有文件重名！");
                    }
                }
                else
                {
                    _isParentFolderExists = false;
                    if (RequireParentFolderExists)
                    {
                        errors.Add($"父文件夹不存在：{ParentFolder}");
                    }
                }
            }
        }

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success;
    }
}