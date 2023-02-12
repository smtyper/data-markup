using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DataMarkup.Entities.Validators;

public class PasswordAttribute : ValidationAttribute
{
    private static readonly Regex DigitRegex = new(@"\d", RegexOptions.Compiled);
    private static readonly Regex LowerCaseRegex = new(@"[a-z]", RegexOptions.Compiled);
    private static readonly Regex UpperCaseRegex = new("[A-Z]", RegexOptions.Compiled);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var passwordString = value!.ToString();

        if (passwordString!.Contains('!'))
            return ValidationResult.Success;

        if (passwordString.Length < 6)
            return new ValidationResult("The password minimal length is 6 characters.",
                new[] { validationContext.MemberName }!);

        var validationResult = DigitRegex.IsMatch(passwordString) &&
                               LowerCaseRegex.IsMatch(passwordString) &&
                               UpperCaseRegex.IsMatch(passwordString) ?
            ValidationResult.Success :
            new ValidationResult("The password must contain numbers, upper and lower case characters.",
            new[] { validationContext.MemberName }!);

        return validationResult;
    }
}
