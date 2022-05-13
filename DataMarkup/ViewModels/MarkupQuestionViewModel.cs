using System.ComponentModel.DataAnnotations;

namespace DataMarkup.ViewModels;

public class MarkupQuestionViewModel
{
    [Required]
    [Display(Name = "Static content")]
    public string StaticContent { get; set; }

    [Required]
    [Display(Name = "Dynamic content regex")]
    public string DynamicContentConstraint { get; set; }

    [Required]
    [Display(Name = "Answer description")]
    public string AnswerDescription { get; set; }

    [Required]
    [Display(Name = "Answer content regex")]
    public string AnswerConstraint { get; set; }

    [Display(Name = "Contains image")]
    public bool ContainsImage { get; set; }
}
