namespace DataMarkup.Frontend.Models;

public record UserSession
{
    public string Username { get; init; } = null!;

    public string Token { get; init; } = null!;

    public DateTime Expiration { get; init; }
}
