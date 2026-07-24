using System.ComponentModel.DataAnnotations;

namespace SmakoszApp.Models
{
    public class UserSettingsViewModel
    {
        [Required(ErrorMessage = "Login jest wymagany.")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Niepoprawny format adresu e-mail.")]
        public string Email { get; set; } = string.Empty;

        public string? CurrentProfilePicture { get; set; }

        public string ThemePreference { get; set; } = "light";

        // Pola zmiany hasła (opcjonalne)
        public string? CurrentPassword { get; set; }

        [MinLength(6, ErrorMessage = "Nowe hasło musi mieć co najmniej 6 znaków.")]
        public string? NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Hasła nie są identyczne.")]
        public string? ConfirmNewPassword { get; set; }
    }
}