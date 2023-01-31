using DataMarkup.Api.Models.Database.Access;
using DataMarkup.Api.Models.Database.Account;
using DataMarkup.Entities;

namespace DataMarkup.Api.Models.Database.Markup;

public record TaskType
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public int SolutionsCount { get; init; }

    public decimal Payment { get; init; }

    public string Instruction { get; init; } = null!;

    public AccessType AccessType { get; init; }

    public User? User { get; init; }

    public string UserId { get; init; } = null!;

    public IReadOnlyCollection<QuestionType>? QuestionTypes { get; init; }

    public IReadOnlyCollection<TaskInstance>? TaskInstances { get; init; }

    public IReadOnlyCollection<Permission>? Permissions { get; init; }
}

public record QuestionType
{
    public Guid Id { get; init; }

    public string StaticContent { get; init; } = null!;

    public string DynamicContentConstraint { get; init; } = null!;

    public string AnswerDescription { get; init; } = null!;

    public string AnswerConstraint { get; init; } = null!;

    public Guid TaskTypeId { get; init; }

    public TaskType? TaskType { get; init; }

    public IReadOnlyCollection<QuestionInstance>? QuestionInstances { get; init; }
}
