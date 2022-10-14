using System.Text.RegularExpressions;

namespace DataMarkup.Api;

public static class Extentions
{
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
