using System.ComponentModel.DataAnnotations;

namespace DataMarkup.ViewModels;

public record LoginViewModel
{
    [Required]
    [Display(Name = "Username")]
    public string UserName { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [Display(Name = "Remember?")]
    public bool Remember { get; set; }

    public string ReturnUrl { get; set; }
}
