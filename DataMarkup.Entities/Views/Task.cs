namespace DataMarkup.Entities.Views;

public record Task
{
    public Guid InstanceId { get; init; }

    public string Instruction { get; init; } = null!;

    public IReadOnlyCollection<Question> Questions { get; init; } = null!;
}

public record Question
{
    public Guid InstanceId { get; init; }

    public string Description { get; init; } = null!;

    public string Content { get; init; } = null!;

    public string AnswerDescription { get; init; } = null!;

    public string AnswerPattern { get; init; } = null!;
}
