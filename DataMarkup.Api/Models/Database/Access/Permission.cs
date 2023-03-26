using DataMarkup.Api.Models.Database.Account;
using DataMarkup.Api.Models.Database.Markup;

namespace DataMarkup.Api.Models.Database.Access;

public record Permission
{
    public Guid Id { get; init; }

    public Guid TaskTypeId { get; init; }

    public TaskType? TaskType { get; init; }

    public string UserId { get; init; } = null!;

    public string Username { get; init; } = null!;

    public User? User { get; init; }
}
