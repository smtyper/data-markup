using DataMarkup.Api.Models.Markup;
using Microsoft.AspNetCore.Identity;

namespace DataMarkup.Api.Models.Account;

public class User : IdentityUser
{
    public IReadOnlyCollection<TaskType> TaskTypes { get; init; } = null!;
}
