using DataMarkup.Api.Models.Account;

namespace DataMarkup.Api.Models.Markup;

public record TaskType
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public int SolutionsCount { get; init; }

    public decimal Payment { get; init; }

    public string Instruction { get; init; } = null!;

    public User User { get; init; } = null!;

    public IReadOnlyCollection<QuestionType> QuestionTypes { get; init; } = null!;
}

public record QuestionType
{
    public Guid Id { get; init; }

    public string StaticContent { get; init; } = null!;

    public string DynamicContentConstraint { get; init; } = null!;

    public string AnswerDescription { get; init; } = null!;

    public string AnswerConstraint { get; init; } = null!;

    public bool ContainsImage { get; init; }

    public TaskType TaskType { get; set; } = null!;
}
