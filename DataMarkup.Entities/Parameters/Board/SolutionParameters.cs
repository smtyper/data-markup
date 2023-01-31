using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Entities.Parameters.Board;

public record SolutionParameters
{
    [Required]
    public Guid? TaskInstanceId { get; set; }

    [Required]
    public IReadOnlyCollection<AnswerParameters> Answers { get; set; } = null!;
}

public record AnswerParameters
{
    [Required]
    public Guid? QuestionInstanceId { get; set; }

    [Required]
    public string Content { get; set; } = null!;
}
