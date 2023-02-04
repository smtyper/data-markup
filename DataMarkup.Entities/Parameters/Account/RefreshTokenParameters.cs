using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Entities.Parameters.Account;

public record RefreshTokenParameters
{
    [Required]
    public string? RefreshToken { get; init; }
}
