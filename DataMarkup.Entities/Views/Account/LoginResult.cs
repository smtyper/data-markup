namespace DataMarkup.Entities.Views.Account;

public record LoginResult : RequestResult
{
    public string? Token { get; init; }

    public string? RefreshToken { get; init; }

    public DateTime? TokenExpiration { get; init; }

    public DateTime? RefreshTokenExpiration { get; init; }
}
