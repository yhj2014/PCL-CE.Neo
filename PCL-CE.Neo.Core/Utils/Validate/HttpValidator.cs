using FluentValidation;
using FluentValidation.Results;
using PCL_CE.Neo.Core.Utils;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class HttpValidator(bool allowNullOrEmpty) : AbstractValidator<string>
{
    public bool AllowsNullOrEmpty { get; set; } = allowNullOrEmpty;

    public HttpValidator() : this(false)
    {
    }

    private void BuildRules()
    {
        RuleFor(x => x)
            .Must(x =>
            {
                if (AllowsNullOrEmpty && string.IsNullOrEmpty(x))
                {
                    return true;
                }

                return RegexPatterns.HttpUri.IsMatch(x);
            }).WithMessage("输入的网址无效！");
    }

    protected override bool PreValidate(ValidationContext<string> context, ValidationResult result)
    {
        BuildRules();
        return base.PreValidate(context, result);
    }
}
