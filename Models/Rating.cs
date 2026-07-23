namespace SmakoszApp.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public string ExternalMealId { get; set; } = string.Empty;
        public int Score { get; set; }
        public int? UserId { get; set; }
        public string? UserIp { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}