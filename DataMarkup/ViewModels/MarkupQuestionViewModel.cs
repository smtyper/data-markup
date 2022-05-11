using System.ComponentModel.DataAnnotations;

namespace DataMarkup.ViewModels;

public class MarkupQuestionViewModel
{
    [Display(Name = "Static content")]
    public string StaticContent { get; set; }

    [Display(Name = "Dynamic content regex")]
    public string DynamicContentConstraint { get; set; }

    [Display(Name = "Aswer description")]
    public string AswerDescription { get; set; }

    [Display(Name = "Answer content regex")]
    public string AnswerConstraint { get; set; }

    [Display(Name = "Contains image")]
    public bool ContainsImage { get; set; }
}
