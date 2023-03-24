namespace DataMarkup.Entities.Views;

public record TaskStatistic
{
    public Guid Id { get; init; }

    public IReadOnlyCollection<Solution> Solutions { get; init; } = null!;

    public IReadOnlyCollection<QuestionContent> QuestionContents { get; init; } = null!;
}

public record QuestionContent
{
    public Guid Id { get; init; }

    public string Content { get; init; } = null!;
}

public record Solution
{
    public Guid Id { get; init; }

    public string Username { get; init; } = null!;

    public IReadOnlyCollection<Answer> Answers { get; init; } = null!;
}

public record Answer
{
    public Guid QuestionId { get; init; }

    public string Content { get; init; } = null!;
}
