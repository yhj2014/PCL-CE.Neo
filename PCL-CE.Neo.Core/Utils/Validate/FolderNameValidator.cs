using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PCL.CE.Neo.Core.Utils.Validate;

public class FolderNameValidator : IValidator<string>
{
    public bool UseMinecraftCharCheck { get; set; } = true;
    public bool IgnoreCase { get; set; } = true;
    public string? ParentFolder { get; set; }
    public bool RequireParentFolderExists { get; set; } = true;

    public ValidationResult Validate(string folderName)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(folderName))
        {
            errors.Add("输入内容不能为空！");
        }

        if (!errors.Any())
        {
            if (folderName.StartsWith(' '))
                errors.Add("文件夹名不能以空格开头！");

            if (folderName.EndsWith(' '))
                errors.Add("文件夹名不能以空格结尾！");

            if (UseMinecraftCharCheck)
            {
                foreach (var invalidChar in new[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*', '!', ';' })
                {
                    if (folderName.Contains(invalidChar))
                    {
                        errors.Add($"文件夹名不可包含 {invalidChar} 字符！");
                        break;
                    }
                }
            }

            if (ParentFolder is not null)
            {
                var dirInfo = new DirectoryInfo(ParentFolder);
                if (dirInfo.Exists)
                {
                    if (dirInfo.EnumerateDirectories().Any(d =>
                        string.Equals(d.Name, folderName,
                            IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)))
                    {
                        errors.Add("不可与现有文件夹重名！");
                    }
                }
                else
                {
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