namespace DataMarkup.Entities.Views;

public record SolverTask
{
    public Guid InstanceId { get; init; }

    public string Instruction { get; init; } = null!;

    public IReadOnlyCollection<SolverQuestion> Questions { get; init; } = null!;
}

public record SolverQuestion
{
    public Guid InstanceId { get; init; }

    public string Description { get; init; } = null!;

    public string Content { get; init; } = null!;

    public string? ImageContent { get; init; }

    public string AnswerDescription { get; init; } = null!;

    public string AnswerPattern { get; init; } = null!;
}
