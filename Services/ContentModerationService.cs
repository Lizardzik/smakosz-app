using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SmakoszApp.Services
{
    public interface IContentModerationService
    {
        Task<(bool IsAllowed, string Reason)> ValidateTextAsync(string input);
    }

    public class ContentModerationService : IContentModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly List<string> _bannedKeywords = new();

        public ContentModerationService(HttpClient httpClient, IConfiguration configuration, IWebHostEnvironment env)
        {
            _httpClient = httpClient;

            // Pobieramy klucz z konfiguracji
            _apiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;

            // 1. Ładowanie lokalnej listy zakazanych słów
            string filePath = Path.Combine(env.ContentRootPath, "banned-words.txt");
            if (File.Exists(filePath))
            {
                try
                {
                    var lines = File.ReadAllLines(filePath);
                    foreach (var line in lines)
                    {
                        string trimmed = RemoveDiacritics(line.Trim().ToLower());
                        if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("#"))
                        {
                            _bannedKeywords.Add(trimmed);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Moderation TXT Error]: {ex.Message}");
                }
            }
        }

        public async Task<(bool IsAllowed, string Reason)> ValidateTextAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (true, string.Empty);

            string normalizedInput = RemoveDiacritics(input.ToLower());

            // 1. LOKALNY FILTR SŁÓW
            foreach (var banned in _bannedKeywords)
            {
                bool isBlocked = false;

                if (banned.Length >= 3)
                {
                    isBlocked = normalizedInput.Contains(banned);
                }
                else
                {
                    string pattern = $@"\b{Regex.Escape(banned)}\b";
                    isBlocked = Regex.IsMatch(normalizedInput, pattern);
                }

                if (isBlocked)
                {
                    Console.WriteLine($"[Moderation LOCAL] ZABLOKOWANO: Wykryto '{banned}' w tekście.");
                    return (false, "Local filter: Prohibited content detected.");
                }
            }

            // 2. OPENAI OMNI-MODERATION (Najnowszy model wyłapujący groźby i 'kill yourself')
            if (string.IsNullOrEmpty(_apiKey) || _apiKey == "API_KEY" || _apiKey == "ENV_VAR")
            {
                Console.WriteLine("[Moderation AI Warning] Brak klucza API! Zapytanie OpenAI pominięte.");
                return (true, string.Empty);
            }

            try
            {
                // Używamy omni-moderation-latest zamiast domyślnego
                var requestBody = new
                {
                    model = "omni-moderation-latest",
                    input = input
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/moderations");
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");
                request.Content = jsonContent;

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string errBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Moderation AI HTTP Error {response.StatusCode}]: {errBody}");
                    return (true, string.Empty);
                }

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);

                bool flagged = doc.RootElement
                    .GetProperty("results")[0]
                    .GetProperty("flagged")
                    .GetBoolean();

                if (flagged)
                {
                    Console.WriteLine($"[Moderation AI] ZABLOKOWANO przez OpenAI (omni-moderation): '{input}'");
                    return (false, "AI filter: Inappropriate or harmful content detected.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Moderation AI Exception]: {ex.Message}");
            }

            return (true, string.Empty);
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}