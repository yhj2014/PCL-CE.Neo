namespace PCL_CE.Neo.Core.Utils.Validate;

public interface IValidator<in T>
{
    bool Validate(T? value);
    string GetErrorMessage(T? value);
}