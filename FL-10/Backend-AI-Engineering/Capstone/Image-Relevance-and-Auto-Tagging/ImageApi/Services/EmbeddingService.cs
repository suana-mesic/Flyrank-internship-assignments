using System.Text.Json;

namespace ImageApi.Services
{
    public class EmbeddingService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly string _model;

        public EmbeddingService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _baseUrl = config["AI:OllamaUrl"] ?? "http://localhost:11434";
            _model = config["AI:EmbedModel"] ?? "nomic-embed-text";
        }

        public async Task<(float[] embedding, int tokens)> EmbedAsync(string text)
        {
            var body = new { model = _model, input = text };

            var resp = await _http.PostAsJsonAsync($"{_baseUrl}/api/embed", body);
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Ollama embed {(int)resp.StatusCode}: {err}");
            }

            var root = await resp.Content.ReadFromJsonAsync<JsonElement>();

            // /api/embed returns "embeddings" as an array of vectors; we sent one input.
            var embedding = root.GetProperty("embeddings")[0]
               .EnumerateArray()
               .Select(x => (float)x.GetDouble())
               .ToArray();

            var tokens = root.TryGetProperty("prompt_eval_count", out var p) ? p.GetInt32() : 0;
            return (embedding, tokens);
        }
    }
}
