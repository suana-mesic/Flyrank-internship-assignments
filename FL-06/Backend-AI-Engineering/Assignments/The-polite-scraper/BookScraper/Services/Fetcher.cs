namespace BookScraper.Services
{
    public static class Fetcher
    {
        private const int DelayMs = 1000;

        public static async Task<string> GetPageAsync(HttpClient http, string url)
        {
            Console.WriteLine($"Feztching: {url}");

            var html = await http.GetStringAsync(url);

            await Task.Delay(DelayMs);

            return html;
        }
    }
}
