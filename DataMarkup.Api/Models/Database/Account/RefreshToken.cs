namespace DataMarkup.Api.Models.Database.Account;

public record RefreshToken
{
    public Guid Id { get; init; }

    public string Token { get; init; } = null!;

    public string UserId { get; init; } = null!;

    public User User { get; init; } = null!;

    public DateTime Expiration { get; init; }
}
