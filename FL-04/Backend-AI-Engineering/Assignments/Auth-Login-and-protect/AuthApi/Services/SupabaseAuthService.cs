using System.Text.Json;

namespace AuthApi.Services
{
    public class SupabaseAuthService
    {
        private readonly HttpClient _http;
        private readonly string _url;
        private readonly string _key;

        public SupabaseAuthService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _url = config["SUPABASE_URL"]
                ?? throw new InvalidOperationException("Missing SUPABASE_URL");
            _key = config["SUPABASE_KEY"]
                ?? throw new InvalidOperationException("Missing SUPABASE_KEY");
        }
        private HttpRequestMessage CreateRequest(HttpMethod method, string path, object? body = null)
        {
            var request = new HttpRequestMessage(method, $"{_url}{path}");
            request.Headers.Add("apikey", _key);

            if (body is not null)
                request.Content = JsonContent.Create(body);

            return request;
        }


        public async Task<JsonElement> SignUpAsync(string email, string password)
        {
            var request = CreateRequest(HttpMethod.Post, "/auth/v1/signup",
                new { email, password });

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            return response.IsSuccessStatusCode
                ? json
                : throw new Exception(json.GetProperty("msg").GetString() ?? "Signup failed");
        }


        public async Task<JsonElement> SignInAsync(string email, string password)
        {
            var request = CreateRequest(HttpMethod.Post,
                "/auth/v1/token?grant_type=password",
                new { email, password });

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (!response.IsSuccessStatusCode)
                throw new UnauthorizedAccessException("Invalid login credentials");

            return json;
        }

        public async Task<JsonElement> GetUserAsync(string token)
        {
            var request = CreateRequest(HttpMethod.Get, "/auth/v1/user");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new UnauthorizedAccessException("Invalid or expired token");

            return await response.Content.ReadFromJsonAsync<JsonElement>();
        }


        public async Task LogoutAsync(string token)
        {
            var request = CreateRequest(HttpMethod.Post, "/auth/v1/logout");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}
