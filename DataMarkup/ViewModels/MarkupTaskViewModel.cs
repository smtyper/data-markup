using System.ComponentModel.DataAnnotations;

namespace DataMarkup.ViewModels;

public class MarkupTaskViewModel
{
    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; }

    [Required]
    [Display(Name = "Max solutions")]
    [Range(1, 100)]
    public int MaxSolutions { get; set; }

    [Required]
    [Display(Name = "Payment")]
    [Range(0, 1000)]
    public decimal Payment { get; set; }

    [Required]
    [Display(Name = "Instruction")]
    public string Instruction { get; set; }

    public MarkupQuestionViewModel CurrentQuestion { get; set; }

    public List<MarkupQuestionViewModel> Questions { get; set; }
}
