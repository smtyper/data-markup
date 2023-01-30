namespace DataMarkup.Api.Models.Views.Account;

public record LoginResult
{
    public bool Succesful { get; init; }

    public string? Message { get; init; }

    public string? Token { get; init; }

    public DateTime? Expiration { get; init; }
}
