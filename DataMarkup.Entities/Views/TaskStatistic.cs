namespace DataMarkup.Entities.Views;

public record Task
{
    public Guid Id { get; init; }

    public int SolutionCount { get; init; }

    public int MaxSolutionCount { get; init; }

    public decimal TaskSolvingPercent { get; init; }

    public IReadOnlyCollection<Question> Questions { get; init; } = null!;
}

public record Question
{
    public string QuestionWording { get; init; } = null!;

    public string Content { get; init; } = null!;

    public string? Image { get; init; }

    public IReadOnlyCollection<Answer> Answers { get; init; } = null!;

    public IReadOnlyCollection<AnswerStatistic> AnswerStatistics { get; init; } = null!;

    public AnswerStatistic? RelevantAnswerStatistic { get; init; } = null!;
}

public record Answer
{
    public string Username { get; init; } = null!;

    public string Content { get; init; } = null!;
}

public record AnswerStatistic
{
    public string Answer { get; init; } = null!;

    public decimal Frequency { get; init; }
}
