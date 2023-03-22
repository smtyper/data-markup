namespace DataMarkup.Entities.Views;

public record Permission
{
    public Guid UserId { get; init; }

    public string Username { get; init; } = null!;
}
