using Microsoft.AspNetCore.Identity;

namespace DataMarkup.Models;

public class User : IdentityUser<Guid>
{
    public DateTime BirthDate { get; set; }

    public List<MarkupTask> MarkupTasks { get; set; }

    public User()
    {
    }
}
