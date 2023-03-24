using DataMarkup.Api.Models.Database.Account;

namespace DataMarkup.Api.Models.Database.Markup;

public record Solution
{
    public Guid Id { get; init; }

    public Guid TaskInstanceId { get; init; }

    public TaskInstance? TaskInstance { get; init; }

    public Guid UserId { get; init; }

    public User? User { get; set; }

    public IReadOnlyCollection<Answer>? Answers { get; init; } = null!;
}

public record Answer
{
    public Guid Id { get; init; }

    public string Content { get; init; } = null!;

    public Guid SolutionId { get; init; }

    public Solution? Solution { get; init; }

    public Guid QuestionInstanceId { get; init; }

    public QuestionInstance? QuestionInstance { get; init; }
}
