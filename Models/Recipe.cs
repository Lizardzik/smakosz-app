using System.ComponentModel.DataAnnotations;

namespace SmakoszApp.Models
{
    public class Recipe
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Podaj nazwę przepisu")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Podaj sposób przygotowania")]
        public string Instructions { get; set; }

        public string? ImagePath { get; set; }

        [Required(ErrorMessage = "Podaj czas przygotowania")]
        public int PrepTimeMinutes { get; set; }

        [Required(ErrorMessage = "Podaj kalorie")]
        public int Calories { get; set; }

        public string? MainCategory { get; set; }
        public string? SubCategory { get; set; }
        public string? DietType { get; set; }
        public string? CuisineType { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    }
}