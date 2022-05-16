using Microsoft.AspNetCore.Identity;

namespace DataMarkup.Models;

public class User : IdentityUser<Guid>
{
    public DateTime BirthDate { get; init; }

    public List<MarkupTask> MarkupTasks { get; init; }

    public User()
    {
    }
}
