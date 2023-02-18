namespace DataMarkup.Frontend.Models;

public record Alert
{
    public bool Show { get; init; }

    public string Message { get; init; } = null!;

    public AlertType Type { get; init; }

    public string TypeString => Type switch
    {
        AlertType.Success => "Success",
        AlertType.Danger => "Danger",
        AlertType.Warning => "Warning",
        _ => throw new ArgumentOutOfRangeException()
    };
}

public enum AlertType
{
    Success,
    Danger,
    Warning
}
