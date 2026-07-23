using System.Text.Json.Serialization;

namespace SmakoszApp.Models
{
    public class ExternalMeal
    {
        [JsonPropertyName("idMeal")]
        public string IdMeal { get; set; } = string.Empty;

        [JsonPropertyName("strMeal")]
        public string StrMeal { get; set; } = string.Empty;

        [JsonPropertyName("strCategory")]
        public string StrCategory { get; set; } = string.Empty;

        [JsonPropertyName("strArea")]
        public string StrArea { get; set; } = string.Empty;

        [JsonPropertyName("strInstructions")]
        public string StrInstructions { get; set; } = string.Empty;

        [JsonPropertyName("strMealThumb")]
        public string StrMealThumb { get; set; } = string.Empty;

        [JsonPropertyName("strYoutube")]
        public string StrYoutube { get; set; } = string.Empty;

        // Składniki 1-10 (dla czytelności najważniejsze)
        [JsonPropertyName("strIngredient1")] public string? StrIngredient1 { get; set; }
        [JsonPropertyName("strIngredient2")] public string? StrIngredient2 { get; set; }
        [JsonPropertyName("strIngredient3")] public string? StrIngredient3 { get; set; }
        [JsonPropertyName("strIngredient4")] public string? StrIngredient4 { get; set; }
        [JsonPropertyName("strIngredient5")] public string? StrIngredient5 { get; set; }
        [JsonPropertyName("strIngredient6")] public string? StrIngredient6 { get; set; }
        [JsonPropertyName("strIngredient7")] public string? StrIngredient7 { get; set; }
        [JsonPropertyName("strIngredient8")] public string? StrIngredient8 { get; set; }

        // Miary/Ilości
        [JsonPropertyName("strMeasure1")] public string? StrMeasure1 { get; set; }
        [JsonPropertyName("strMeasure2")] public string? StrMeasure2 { get; set; }
        [JsonPropertyName("strMeasure3")] public string? StrMeasure3 { get; set; }
        [JsonPropertyName("strMeasure4")] public string? StrMeasure4 { get; set; }
        [JsonPropertyName("strMeasure5")] public string? StrMeasure5 { get; set; }
        [JsonPropertyName("strMeasure6")] public string? StrMeasure6 { get; set; }
        [JsonPropertyName("strMeasure7")] public string? StrMeasure7 { get; set; }
        [JsonPropertyName("strMeasure8")] public string? StrMeasure8 { get; set; }

        // Metoda pomocnicza tworząca czystą listę składników z miarami
        public List<string> GetIngredientsList()
        {
            var list = new List<string>();
            var ingredients = new[] { StrIngredient1, StrIngredient2, StrIngredient3, StrIngredient4, StrIngredient5, StrIngredient6, StrIngredient7, StrIngredient8 };
            var measures = new[] { StrMeasure1, StrMeasure2, StrMeasure3, StrMeasure4, StrMeasure5, StrMeasure6, StrMeasure7, StrMeasure8 };

            for (int i = 0; i < ingredients.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(ingredients[i]))
                {
                    var measure = !string.IsNullOrWhiteSpace(measures[i]) ? measures[i] : "";
                    list.Add($"{ingredients[i]} - {measure}".TrimEnd('-', ' '));
                }
            }
            return list;
        }
    }

    public class MealDbResponse
    {
        [JsonPropertyName("meals")]
        public List<ExternalMeal>? Meals { get; set; }
    }
}