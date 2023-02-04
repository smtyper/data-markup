namespace DataMarkup.Entities.Views.Account;

public record RefreshTokenResult : RequestResult
{
    public string? Token { get; init; }

    public DateTime? Expiration { get; init; }
}
