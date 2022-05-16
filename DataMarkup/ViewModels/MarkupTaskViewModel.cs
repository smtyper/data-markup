using System.ComponentModel.DataAnnotations;

namespace DataMarkup.ViewModels;

public record MarkupTaskViewModel
{
    public Guid Id { get; init; }

    [Required]
    [Display(Name = "Task name")]
    public string Name { get; set; }

    [Required]
    [Range(1, 100)]
    [Display(Name = "Max solution count")]
    public int MaxSolutions { get; set; }

    [Required]
    [Range(0, 1000)]
    [Display(Name = "Payment for one task instance")]
    public decimal Payment { get; set; }

    [Required]
    [Display(Name = "Task instruction")]
    public string Instruction { get; set; }

    public MarkupQuestionViewModel CurrentQuestion { get; set; }

    public List<MarkupQuestionViewModel> Questions { get; set; }
}
