using System.ComponentModel.DataAnnotations;

namespace ProductManagement.WebUI.Models.Auth;

public sealed class LoginViewModel
{
    [Display(Name = "Email")]
    [Required(ErrorMessage = "Email zorunludur.")]
    [StringLength(256)]
    public string UserNameOrEmail { get; set; } = string.Empty;

    [Display(Name = "Sifre")]
    [Required(ErrorMessage = "Sifre zorunludur.")]
    [DataType(DataType.Password)]
    [StringLength(128, MinimumLength = 6, ErrorMessage = "Sifre en az 6 karakter olmalidir.")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Beni hatirla")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
