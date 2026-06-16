using System.IO;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class FileNameValidator
{
    private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars();

    public static ValidationResult Validate(string fileName, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return ValidationResult.Failure($"{fieldName} 不能为空");

        if (fileName.IndexOfAny(InvalidChars) >= 0)
            return ValidationResult.Failure($"{fieldName} 包含无效字符");

        if (fileName.Any(c => char.IsControl(c)))
            return ValidationResult.Failure($"{fieldName} 包含控制字符");

        var trimmed = fileName.Trim();
        if (trimmed.EndsWith(".") || trimmed.EndsWith(" "))
            return ValidationResult.Failure($"{fieldName} 不能以点或空格结尾");

        if (trimmed.Length > 255)
            return ValidationResult.Failure($"{fieldName} 长度不能超过 255 个字符");

        return ValidationResult.Success();
    }
}