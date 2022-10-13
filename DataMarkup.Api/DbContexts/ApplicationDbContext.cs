using DataMarkup.Api.Models.Account;
using DataMarkup.Api.Models.Markup;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataMarkup.Api.DbContexts;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public DbSet<TaskType> TaskTypes { get; init; } = null!;

    public DbSet<QuestionType> QuestionTypes { get; init; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TaskType>()
            .HasOne<User>()
            .WithMany(user => user.TaskTypes)
            .HasForeignKey(type => type.UserId);

        builder.Entity<TaskType>()
            .HasMany<QuestionType>()
            .WithOne(questionType => questionType.TaskType);
    }
}
