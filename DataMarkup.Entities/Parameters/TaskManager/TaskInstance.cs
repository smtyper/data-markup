using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Entities.Parameters.TaskManager;

public record TaskInstancesParameters
{
    [Required]
    public Guid TaskTypeId { get; set; }

    [Required]
    public IReadOnlyDictionary<Guid, IReadOnlyList<QuestionInstanceParameters>> QuestionDictionary { get; set; } =
        null!;
}


public record QuestionInstanceParameters
{
    [Required]
    public string Content { get; set; } = null!;

    [DataType(DataType.ImageUrl)]
    public string? ImageSource { get; set; }
}
