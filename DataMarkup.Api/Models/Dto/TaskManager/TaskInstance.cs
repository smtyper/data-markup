using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Api.Models.Dto.TaskManager;

public record TaskInstancesParameters
{
    [Required]
    public Guid TaskTypeId { get; init; }

    [Required]
    public IReadOnlyCollection<TaskInstance> TaskInstances { get; init; } = null!;
}

public record TaskInstance
{
    [Required]
    public IReadOnlyCollection<QuestionInstance> QuestionInstances { get; set; } = null!;
}

public record QuestionInstance
{
    [Required]
    public Guid QuestionTypeId { get; init; }

    [Required]
    public string Content { get; set; } = null!;

    [DataType(DataType.Url)]
    public string? ImageSource { get; init; }
}
