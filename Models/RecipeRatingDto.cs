namespace SmakoszApp.Models
{
    public class RecipeRatingDto
    {
        public string ExternalMealId { get; set; } = string.Empty;
        public double AverageScore { get; set; }
        public int VotesCount { get; set; }
        public int? UserScore { get; set; } 
    }
}