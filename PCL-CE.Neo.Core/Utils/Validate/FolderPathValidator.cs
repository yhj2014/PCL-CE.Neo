using System;
using System.Collections.Generic;

namespace PCL.CE.Neo.Core.Utils.Validate;

public class FolderPathValidator : IValidator<string>
{
    public ValidationResult Validate(string path)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(path))
        {
            errors.Add("路径不能为空！");
        }

        if (!errors.Any())
        {
            try
            {
                var fullPath = Path.GetFullPath(path);
            }
            catch (Exception ex)
            {
                errors.Add($"无效的路径格式：{ex.Message}");
            }

            foreach (var invalidChar in Path.GetInvalidPathChars())
            {
                if (path.Contains(invalidChar))
                {
                    errors.Add($"路径包含非法字符：{invalidChar}");
                    break;
                }
            }
        }

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success;
    }
}