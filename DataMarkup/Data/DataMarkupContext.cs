using DataMarkup.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataMarkup.Data;

public class DataMarkupContext : IdentityDbContext<User, Role, Guid>
{
    public DbSet<MarkupTask> Tasks { get; set; }

    public DbSet<MarkupQuestion> Questions { get; set; }

    public DbSet<MarkupTaskInstance> TaskInstances { get; set; }

    public DbSet<MarkupQuestionInstance> QuestionInstances { get; set; }

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
            .WithMany(task => task.Questions);

        builder.Entity<MarkupTaskInstance>()
            .HasOne(taskInstance => taskInstance.Task)
            .WithMany(task => task.Instances);
        builder.Entity<MarkupQuestionInstance>()
            .HasOne(questionInstance => questionInstance.TaskInstance)
            .WithMany(taskInstance => taskInstance.QuestionInstances);
        builder.Entity<MarkupQuestionInstance>()
            .HasOne(questionInstance => questionInstance.Question)
            .WithMany();

        base.OnModelCreating(builder);
    }
}
