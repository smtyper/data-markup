using System.ComponentModel.DataAnnotations;
using DataMarkup.Entities.Validators;

namespace DataMarkup.Entities.Parameters.Account;

public record RegisterParameters
{
    [Required(ErrorMessage = "Username is required.")]
    public string? Username { get; set; }

    [EmailAddress]
    [Required(ErrorMessage = "Email is required.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [Password]
    public string? Password { get; set; }
}
