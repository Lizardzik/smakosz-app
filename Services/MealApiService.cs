using System.Net.Http.Json;
using SmakoszApp.Models;

namespace SmakoszApp.Services
{
    public class MealApiService
    {
        private readonly HttpClient _httpClient;

        public MealApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ExternalMeal>> SearchMealsAsync(string query)
        {
            var url = $"https://www.themealdb.com/api/json/v1/1/search.php?s={query}";
            var response = await _httpClient.GetFromJsonAsync<MealDbResponse>(url);
            return response?.Meals ?? new List<ExternalMeal>();
        }

        // Metoda pobierająca bogaty katalog dań pod wyliczanie stron (paginację)
        public async Task<List<ExternalMeal>> GetCatalogMealsAsync()
        {
            var letters = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'm', 'p', 's', 't' };
            var allMeals = new List<ExternalMeal>();

            foreach (var letter in letters)
            {
                var url = $"https://www.themealdb.com/api/json/v1/1/search.php?f={letter}";
                var response = await _httpClient.GetFromJsonAsync<MealDbResponse>(url);
                if (response?.Meals != null)
                {
                    allMeals.AddRange(response.Meals);
                }
            }

            // Usuwamy ewentualne duplikaty i mieszamy/sortujemy
            return allMeals.GroupBy(m => m.IdMeal).Select(g => g.First()).OrderBy(m => m.StrMeal).ToList();
        }

        public async Task<ExternalMeal?> GetMealByIdAsync(string id)
        {
            var url = $"https://www.themealdb.com/api/json/v1/1/lookup.php?i={id}";
            var response = await _httpClient.GetFromJsonAsync<MealDbResponse>(url);
            return response?.Meals?.FirstOrDefault();
        }
    }
}