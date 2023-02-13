using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DataMarkup.Entities.Validators;

public class RegexAttribute : ValidationAttribute
{
    private static readonly Regex RegexRegex = new(
        @"^(?:(?:[^?+*{}()[\]\\|]+|\\.|\[(?:\^?\\.|\^[^\\]|[^\\^])(?:[^\]\\]+|\\.)*\]|\((?:\?[:=!]|\?<[=!]|\?>|\?<[^\W\d]\w*>|\?'[^\W\d]\w*')?(?<N>)|\)(?<-N>))(?:(?:[?+*]|\{\d+(?:,\d*)?\})[?+]?)?|\|)*$(?(N)(?!))",
        RegexOptions.Compiled);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var regexString = value?.ToString()!;

        var validationResult = RegexRegex.IsMatch(regexString) ?
            ValidationResult.Success :
            new ValidationResult("The string must be a valid .Net regex.",
                new[] { validationContext.MemberName }!);

        return validationResult;
    }
}
