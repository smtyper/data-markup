using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Entities.Parameters.Account;

public record LoginParameters
{
    [Required(ErrorMessage = "Username is required.")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string? Password { get; set; }
}
