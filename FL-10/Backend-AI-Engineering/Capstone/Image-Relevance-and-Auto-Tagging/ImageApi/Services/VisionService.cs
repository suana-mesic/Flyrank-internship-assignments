using System.Net.Http.Json;
using System.Text.Json;
using ImageApi.Models;

namespace ImageApi.Services;

public sealed class VisionService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _model;
    private readonly string _corpusPath;

    public VisionService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _baseUrl = config["AI:OllamaUrl"] ?? "http://localhost:11434";
        _model = config["AI:VisionModel"] ?? "llava";
        _corpusPath = config["Images:CorpusPath"] ?? throw new InvalidOperationException("Missing Images__CorpusPath");
    }

    public async Task<ImageTags> ClassifyAsync(string filename)
    {
        // 1. Read image bytes and base64-encode them (Ollama takes images as base64).
        var path = Path.Combine(_corpusPath, filename);
        var bytes = await File.ReadAllBytesAsync(path);
        var base64 = Convert.ToBase64String(bytes);

        // 2. Build the request. "format" is a JSON schema -> Ollama structured output,
        //    so the model must answer in our exact shape.
        var requestBody = new
        {
            model = _model,
            prompt = "Look at this image of an animal and return structured tags. " +
                     "subject = the single main animal; category = a coarse class like 'animal'; " +
                     "attributes = 2-5 short visual descriptors; caption = one sentence; " +
                     "confidence = 0..1 how sure you are of the subject.",
            images = new[] { base64 },
            stream = false,
            format = new
            {
                type = "object",
                properties = new
                {
                    subject = new { type = "string" },
                    category = new { type = "string" },
                    attributes = new { type = "array", items = new { type = "string" } },
                    caption = new { type = "string" },
                    confidence = new { type = "number" }
                },
                required = new[] { "subject", "category", "attributes", "caption", "confidence" }
            }
        };

        // 3. Call local Ollama. Long timeout: vision models can be slow on CPU.
        var resp = await _http.PostAsJsonAsync($"{_baseUrl}/api/generate", requestBody);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Ollama {(int)resp.StatusCode}: {body}");
        }

        // 4. Ollama returns the model's answer as a JSON string in the "response" field.
        var root = await resp.Content.ReadFromJsonAsync<JsonElement>();
        var text = root.GetProperty("response").GetString()!;

        // 5. Deserialize that JSON string into our ImageTags record.
        return JsonSerializer.Deserialize<ImageTags>(text,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}