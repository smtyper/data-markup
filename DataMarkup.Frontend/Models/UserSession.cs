namespace DataMarkup.Frontend.Models;

public record UserSession
{
    public string Username { get; init; } = null!;

    public string Token { get; init; } = null!;

    public string RefreshToken { get; init; } = null!;
}
