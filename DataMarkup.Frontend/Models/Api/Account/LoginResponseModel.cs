namespace DataMarkup.Frontend.Models.Api.Account;

public class LoginResponseModel
{
    public string Token { get; init; } = null!;

    public DateTime Expiration { get; init; }
}
