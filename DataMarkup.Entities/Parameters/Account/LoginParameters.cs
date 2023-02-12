using System.ComponentModel.DataAnnotations;
using DataMarkup.Entities.Validators;

namespace DataMarkup.Entities.Parameters.Account;

public record LoginParameters
{
    [Required(ErrorMessage = "Username is required.")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [Password]
    public string? Password { get; set; }
}
