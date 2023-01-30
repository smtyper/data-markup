namespace DataMarkup.Api.Models.Views.Account;

public record RegisterResult
{
    public bool Succesful { get; init; }

    public string? Message { get; init; }
}
