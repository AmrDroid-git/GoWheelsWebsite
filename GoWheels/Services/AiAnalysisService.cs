using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GoWheels.Services
{
    public class AiAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _openRouterKey;

        public AiAnalysisService(IConfiguration configuration)
        {
            _openRouterKey = configuration["OpenRouter:ApiKey"]!;
            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _openRouterKey);

            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:5237");
            _httpClient.DefaultRequestHeaders.Add("X-Title", "GoWheels AI");
        }

        public async Task<string> AnalyzePostAsync(string prompt)
        {
            var requestBody = new
            {
                model = "stepfun/step-3.5-flash:free",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };


            var json = JsonSerializer.Serialize(requestBody);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "https://openrouter.ai/api/v1/chat/completions",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenRouter Error: {error}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);

            var result = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return result ?? "";
        }
    }
}