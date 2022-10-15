using DataMarkup.Api.Models.Database.Markup;
using Microsoft.AspNetCore.Identity;

namespace DataMarkup.Api.Models.Database.Account;

public class User : IdentityUser
{
    public IReadOnlyCollection<TaskType>? TaskTypes { get; init; }

    public IReadOnlyCollection<Solution>? Solutions { get; init; }
}
