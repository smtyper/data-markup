using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Api.Models.Dto.TaskManager;

public record TaskInstancesParameters
{
    [Required]
    public Guid TaskTypeId { get; init; }

    [Required]
    public IReadOnlyDictionary<Guid, IReadOnlyList<QuestionInstance>> QuestionDictionary { get; init; } = null!;
}


public record QuestionInstance
{
    [Required]
    public string Content { get; set; } = null!;

    [DataType(DataType.ImageUrl)]
    public string? ImageSource { get; init; }
}
