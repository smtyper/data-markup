using DataMarkup.Api.Models.Database.Markup;
using Microsoft.AspNetCore.Identity;

namespace DataMarkup.Api.Models.Database.Account;

public class User : IdentityUser
{
    public IReadOnlyCollection<TaskType> TaskTypes { get; init; } = null!;
}
