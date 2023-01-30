using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Frontend.Models.Api.Account;

public record RegisterModel
{
    [Required(ErrorMessage = "Username is required.")]
    public string? Username { get; set; }

    [EmailAddress]
    [Required(ErrorMessage = "Email is required.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}
