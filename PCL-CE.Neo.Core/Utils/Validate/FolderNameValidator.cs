using System.IO;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class FolderNameValidator
{
    private static readonly char[] InvalidChars = Path.GetInvalidPathChars();

    public static ValidationResult Validate(string folderName, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(folderName))
            return ValidationResult.Failure($"{fieldName} 不能为空");

        if (folderName.IndexOfAny(InvalidChars) >= 0)
            return ValidationResult.Failure($"{fieldName} 包含无效字符");

        if (folderName.Any(c => char.IsControl(c)))
            return ValidationResult.Failure($"{fieldName} 包含控制字符");

        var trimmed = folderName.Trim();
        if (trimmed.EndsWith(".") || trimmed.EndsWith(" "))
            return ValidationResult.Failure($"{fieldName} 不能以点或空格结尾");

        if (trimmed.Length > 255)
            return ValidationResult.Failure($"{fieldName} 长度不能超过 255 个字符");

        return ValidationResult.Success();
    }
}