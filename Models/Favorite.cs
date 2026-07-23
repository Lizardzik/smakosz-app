namespace SmakoszApp.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        // ID z zewnętrznego API
        public string ExternalMealId { get; set; } = string.Empty;

        // Podstawowe dane do wyświetlania na karcie
        public string MealName { get; set; } = string.Empty;
        public string MealThumb { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;

        public DateTime AddedAt { get; set; } = DateTime.Now;
    }
}