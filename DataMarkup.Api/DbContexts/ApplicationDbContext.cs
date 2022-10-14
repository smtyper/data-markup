using DataMarkup.Api.Models.Database.Account;
using DataMarkup.Api.Models.Database.Markup;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataMarkup.Api.DbContexts;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public DbSet<TaskType> TaskTypes { get; init; } = null!;

    public DbSet<QuestionType> QuestionTypes { get; init; } = null!;

    public DbSet<TaskInstance> TaskInstances { get; init; } = null!;

    public DbSet<QuestionInstance> QuestionInstances { get; init; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TaskType>()
            .HasOne(type => type.User)
            .WithMany(user => user.TaskTypes)
            .OnDelete(DeleteBehavior.NoAction);;
        builder.Entity<TaskType>()
            .HasMany(type => type.QuestionTypes)
            .WithOne(questionType => questionType.TaskType)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<TaskType>()
            .HasMany(type => type.TaskInstances)
            .WithOne(instance => instance.TaskType)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<QuestionType>()
            .HasMany(type => type.QuestionInstances)
            .WithOne(instance => instance.QuestionType)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<TaskInstance>()
            .HasMany(taskInstance => taskInstance.QuestionInstances)
            .WithOne(questionInstance => questionInstance.TaskInstance)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
