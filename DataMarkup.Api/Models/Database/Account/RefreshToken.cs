namespace DataMarkup.Api.Models.Database.Account;

public record RefreshToken
{
    public Guid Id { get; init; }

    public string Token { get; init; } = null!;

    public Guid UserId { get; init; }

    public User User { get; init; } = null!;

    public DateTime Expiration { get; init; }
}
