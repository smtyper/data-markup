using System.ComponentModel.DataAnnotations;

namespace DataMarkup.ViewModels;

public record RegistrationViewModel
{
    [Required]
    [Display(Name = "Имя пользователя")]
    public string UserName { get; set; }

    [Required]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; }

    [Required]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    [DataType(DataType.Password)]
    [Display(Name = "Подтверждение пароля")]
    public string PasswordConfirm { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Дата рождения")]
    public DateTime BirthDate { get; set; }
}
