using DataMarkup.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataMarkup.Data;

public class DataMarkupContext : IdentityDbContext<User, Role, Guid>
{
    public DbSet<MarkupTask> MarkupTasks { get; set; }

    public DbSet<MarkupQuestion> MarkupQuestions { get; set; }

    public DataMarkupContext(DbContextOptions<DataMarkupContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<MarkupTask>()
            .HasOne(task => task.User)
            .WithMany(user => user.MarkupTasks);
        builder.Entity<MarkupQuestion>()
            .HasOne(question => question.Task)
            .WithMany(task => task.MarkupQuestions);

        base.OnModelCreating(builder);
    }
}
