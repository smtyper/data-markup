namespace DataMarkup.Frontend.Models;

public record Alert
{
    public bool Show { get; init; }

    public string? Title { get; init; }

    public string Message { get; init; } = null!;

    public AlertType Type { get; init; }

    public string TypeString => Type.ToString().ToLower();
}

public enum AlertType
{
    Success,
    Danger,
    Warning
}
