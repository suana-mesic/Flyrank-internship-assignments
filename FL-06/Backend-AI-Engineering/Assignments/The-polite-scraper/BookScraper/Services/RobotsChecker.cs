namespace BookScraper.Services;

public static class RobotsChecker
{
    public static async Task<bool> IsAllowedAsync(HttpClient http, string baseUrl, string path)
    {
        try
        {
            var robotsUrl = $"{baseUrl.TrimEnd('/')}/robots.txt";
            var text = await http.GetStringAsync(robotsUrl);

            Console.WriteLine("--- robots.txt ---");
            Console.WriteLine(text);
            Console.WriteLine("------------------");

            foreach (var line in text.Split('\n'))
            {
                var trimmed = line.Trim().ToLower();

                if (trimmed.StartsWith("disallow:"))
                {
                    var disallowed = trimmed.Replace("disallow:", "").Trim();

                    if (!string.IsNullOrEmpty(disallowed) && path.StartsWith(disallowed))
                    {
                        Console.WriteLine($"Path '{path}' is disallowed by robots.txt");
                        return false;
                    }
                }
            }

            return true;
        }
        catch
        {
            Console.WriteLine("No robots.txt found — proceeding.");
            return true;
        }
    }
}