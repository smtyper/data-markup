using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Api.Models.Dto.Account;

public record LoginModel
{
    [Required(ErrorMessage = "Username is required.")]
    public string? Username { get; init; }

    [Required(ErrorMessage = "Password is required.")]
    public string? Password { get; init; }
}
