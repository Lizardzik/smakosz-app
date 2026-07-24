namespace SmakoszApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int ReputationPoints { get; set; } = 0;

        // Nowe pola do profilu i motywu:
        public string? ProfilePicturePath { get; set; }
        public string ThemePreference { get; set; } = "light";
    }
}