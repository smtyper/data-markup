namespace DataMarkup.Entities.Views;

public abstract record RequestResult
{
    public bool Successful { get; init; }

    public string? Message { get; init; }
}
