using System.Text.RegularExpressions;

namespace DataMarkup.Api;

public static class Extentions
{
    public static bool IsFullMatch(this Regex regex, string text) => regex.Match(text).Value == text;

    public static bool IsValidRegex(this string patternString)
    {
        try
        {
            var unused = new Regex(patternString);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
