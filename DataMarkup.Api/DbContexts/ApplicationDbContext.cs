﻿using DataMarkup.Api.Models.Database.Access;
using DataMarkup.Api.Models.Database.Account;
using DataMarkup.Api.Models.Database.Markup;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataMarkup.Api.DbContexts;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public DbSet<RefreshToken> RefreshTokens { get; init; } = null!;

    public DbSet<TaskType> TaskTypes { get; init; } = null!;

    public DbSet<QuestionType> QuestionTypes { get; init; } = null!;

    public DbSet<Permission> Permissions { get; init; } = null!;

    public DbSet<TaskInstance> TaskInstances { get; init; } = null!;

    public DbSet<QuestionInstance> QuestionInstances { get; init; } = null!;

    public DbSet<Solution> Solutions { get; init; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>()
            .HasOne(user => user.RefreshToken)
            .WithOne(token => token.User)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<TaskType>()
            .HasOne(type => type.User)
            .WithMany(user => user.TaskTypes)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<TaskType>()
            .HasMany(type => type.QuestionTypes)
            .WithOne(questionType => questionType.TaskType)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<TaskType>()
            .HasMany(type => type.TaskInstances)
            .WithOne(instance => instance.TaskType)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<TaskType>()
            .HasMany(type => type.Permissions)
            .WithOne(permission => permission.TaskType)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<QuestionType>()
            .HasMany(type => type.QuestionInstances)
            .WithOne(instance => instance.QuestionType)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<TaskInstance>()
            .HasMany(taskInstance => taskInstance.QuestionInstances)
            .WithOne(questionInstance => questionInstance.TaskInstance)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<TaskInstance>()
            .HasMany(taskInstance => taskInstance.Solutions)
            .WithOne(solution => solution.TaskInstance)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<QuestionInstance>()
            .HasMany(instance => instance.Answers)
            .WithOne(answer => answer.QuestionInstance)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Solution>()
            .HasOne(solution => solution.User)
            .WithMany(user => user.Solutions)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Solution>()
            .HasMany(solution => solution.Answers)
            .WithOne(answer => answer.Solution)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
