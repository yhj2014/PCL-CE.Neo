using System.IO;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class FolderPathValidator
{
    public static ValidationResult Validate(string path, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(path))
            return ValidationResult.Failure($"{fieldName} 不能为空");

        try
        {
            Path.GetFullPath(path);
        }
        catch
        {
            return ValidationResult.Failure($"{fieldName} 不是有效的路径");
        }

        if (path.Length > 260)
            return ValidationResult.Failure($"{fieldName} 路径长度不能超过 260 个字符");

        return ValidationResult.Success();
    }
}