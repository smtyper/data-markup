using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Api.Models.Dto.TaskManager;

public record TaskInstance
{
    [Required]
    public Guid TaskTypeId { get; set; }

    [Required]
    public IReadOnlyCollection<QuestionInstance> QuestionInstances { get; set; } = null!;
}

public record QuestionInstance
{
    [Required]
    public Guid QuestionTypeId { get; init; }

    [Required]
    public string Content { get; set; } = null!;
}
