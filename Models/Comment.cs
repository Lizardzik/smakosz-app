using System.ComponentModel.DataAnnotations;

namespace SmakoszApp.Models
{
    public class Comment
    {
        public int Id { get; set; }

        // ID przepisu z API (np. "52772")
        [Required]
        public string ExternalMealId { get; set; } = string.Empty;

        // Powiązanie z użytkownikiem
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Liczba polubień danego komentarza przez społeczność
        public int LikesCount { get; set; } = 0;
    }
}