using System.Text.RegularExpressions;
using DataMarkup.Api.Models.Database.Account;
using Microsoft.AspNetCore.Identity;

namespace DataMarkup.Api;

public static class Extentions
{
    public static async ValueTask<User> GetUserAsync(this UserManager<User> userManager, HttpContext httpContext) =>
        await userManager.FindByNameAsync(httpContext.User.Identity!.Name);

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
