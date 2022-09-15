using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Api.Models.Dto.TaskManager;

public record MarkupTask
{
    [Required]
    public string Name { get; init; } = null!;

    [Required]
    [Range(1, 100)]
    public int SolutionsCount { get; init; }

    [Range(0.01, 10000)]
    public decimal Payment { get; init; }

    [Required]
    public string Instruction { get; init; } = null!;

    public TYPE Type { get; init; }
}

public record MarkupQuestion
{
    [Required]
    public string StaticContent { get; init; } = null!;

    [Required]
    public string DynamicContentConstraint { get; init; } = null!;

    [Required]
    public string AnswerDescription { get; init; } = null!;

    [Required]
    public string AnswerConstraint { get; init; } = null!;

    [Display(Name = "Contains image?")]
    public bool ContainsImage { get; init; }
}
