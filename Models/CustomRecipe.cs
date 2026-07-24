using System.ComponentModel.DataAnnotations;

namespace SmakoszApp.Models
{
    public class CustomRecipe
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        // ID z zewnętrznego API (MealDb)
        public string ExternalMealId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string ThumbUrl { get; set; } = string.Empty;

        // Składniki zapisane np. po przecinku lub JSON
        public string Ingredients { get; set; } = string.Empty;

        // Zmodyfikowana instrukcja
        public string Instructions { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}