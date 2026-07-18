using System.Text.Json;

namespace BackgroundJobApi.Services;

public class AiService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public AiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["GROQ_API_KEY"]
            ?? throw new InvalidOperationException("Missing GROQ_API_KEY");
    }

    public async Task<string> SummarizeAsync(string text, CancellationToken ct)
    {
        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var body = new
        {
            model = "llama-3.1-8b-instant",
            messages = new[]
            {
                new { role = "user", content = $"Summarize the following text in 2-3 sentences:\n\n{text}" }
            }
        };

        var response = await _http.PostAsJsonAsync(
            "https://api.groq.com/openai/v1/chat/completions", body, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        return json
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "No response.";
    }
}