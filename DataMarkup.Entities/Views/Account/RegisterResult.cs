namespace DataMarkup.Entities.Views.Account;

public record RegisterResult
{
    public bool Succesful { get; init; }

    public string? Message { get; init; }
}
