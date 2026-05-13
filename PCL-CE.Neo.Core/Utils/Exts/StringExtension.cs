using System.Text;

namespace PCL_CE.Neo.Core.Utils.Exts;

/// <summary>
/// String extension methods.
/// </summary>
public static class StringExtension
{
    /// <summary>
    /// Replaces all line break characters with the specified line break.
    /// </summary>
    public static string ReplaceLineBreak(this string str, string lineBreak)
    {
        var sb = new StringBuilder(str.Length);
        var i = 0;
        while (i < str.Length)
        {
            if (str[i] == '\r')
            {
                if (i + 1 < str.Length && str[i + 1] == '\n')
                {
                    sb.Append(lineBreak);
                    i += 2;
                }
                else
                {
                    sb.Append(lineBreak);
                    i++;
                }
            }
            else if (str[i] == '\n')
            {
                sb.Append(lineBreak);
                i++;
            }
            else
            {
                sb.Append(str[i]);
                i++;
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Normalizes line breaks to CRLF.
    /// </summary>
    public static string NormalizeLineBreak(this string str) => str.ReplaceLineBreak("\r\n");
}
