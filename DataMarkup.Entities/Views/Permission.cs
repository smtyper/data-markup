namespace DataMarkup.Entities.Views;

public record Permission
{
    public string UserId { get; init; } = null!;

    public string Username { get; init; } = null!;
}
