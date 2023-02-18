using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Entities.Parameters.TaskManager;

public record UpdateTaskTypeParameters
{
    [Required]
    public Guid TaskTypeId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    [Range(1, 100)]
    public int SolutionsCount { get; set; }

    [Required]
    [Range(0.01, 1000)]
    public decimal Payment { get; set; }

    [Required]
    public string Instruction { get; set; } = null!;

    [Required]
    public AccessType AccessType { get; set; }
}
