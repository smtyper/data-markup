using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Api.Models.Dto.Board;

public record Solution
{
    [Required]
    public Guid? TaskInstanceId { get; init; }

    [Required]
    public IReadOnlyCollection<Answer> Answers { get; init; } = null!;
}

public record Answer
{
    [Required]
    public Guid? QuestionInstanceId { get; init; }

    [Required]
    public string Content { get; init; } = null!;
}
