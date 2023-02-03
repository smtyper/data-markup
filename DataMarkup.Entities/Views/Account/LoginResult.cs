namespace DataMarkup.Entities.Views.Account;

public record LoginResult : RequestResult
{
    public string? Token { get; init; }

    public string? RefreshToken { get; init; }

    public DateTime? Expiration { get; init; }
}
