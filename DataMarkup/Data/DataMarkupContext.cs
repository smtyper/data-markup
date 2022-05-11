using DataMarkup.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataMarkup.Data;

public class DataMarkupContext : IdentityDbContext<User, Role, Guid>
{
    public DataMarkupContext(DbContextOptions<DataMarkupContext> options) : base(options)
    {
    }
}
