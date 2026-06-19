namespace PCL_CE.Neo.Core.Utils.Validate;

public static class RegexPatterns
{
    public const string HttpUri = @"^(https?):\/\/[^\s/$.?#].[^\s]*$";
    
    public const string UncPath = @"^\\\\[\w-]+(\\[\w-.]+)*$";
    
    public const string Ntfs83FileName = @"^[^\x00-\x1F\x7F:<>""|]*[^\x00-\x1F\x7F:<>""| .]{1,8}\.[^\x00-\x1F\x7F:<>""| .]{1,3}$";
}