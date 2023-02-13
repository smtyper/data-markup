using System.ComponentModel.DataAnnotations;
using DataMarkup.Entities.Validators;

namespace DataMarkup.Entities.Parameters.TaskManager;

public record TaskTypeParameters
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    [Range(1, 100)]
    public int SolutionsCount { get; set; }

    [Range(0.01, 1000)]
    public decimal Payment { get; set; }

    [Required]
    public string Instruction { get; set; } = null!;

    [Required]
    public AccessType? AccessType { get; set; }

    [Required]
    public IReadOnlyCollection<QuestionTypeParameters> Questions { get; set; } = null!;
}

public record QuestionTypeParameters
{
    [Required]
    public string StaticContent { get; set; } = null!;

    [Required]
    [Regex]
    public string DynamicContentConstraint { get; set; } = null!;

    [Required]
    public string AnswerDescription { get; set; } = null!;

    [Required]
    [Regex]
    public string AnswerConstraint { get; set; } = null!;
}
