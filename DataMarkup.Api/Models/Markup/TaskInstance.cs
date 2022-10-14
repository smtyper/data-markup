namespace DataMarkup.Api.Models.Markup;

public record TaskInstance
{
    public Guid Id { get; init; }

    public Guid TaskTypeId { get; init; }

    public TaskType? TaskType { get; init; }

    public IReadOnlyCollection<QuestionInstance>? QuestionInstances { get; init; }
}

public record QuestionInstance
{
    public Guid Id { get; init; }

    public string Content { get; init; } = null!;

    public Guid TaskInstanceId { get; init; }

    public TaskInstance? TaskInstance { get; init; }

    public Guid QuestionTypeId { get; init; }

    public QuestionType? QuestionType { get; init; }
}
