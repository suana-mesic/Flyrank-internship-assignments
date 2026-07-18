using BookScraper.Models;
using HtmlAgilityPack;

namespace BookScraper.Services
{
    public static class Parser
    {
        public static List<string> GetBookLinks(string html, string baseUrl)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var links = new List<string>();

            var nodes = doc.DocumentNode.SelectNodes("//article[@class='product_pod']//h3/a");

            if (nodes is null) return links;

            foreach (var node in nodes)
            {
                var href = node.GetAttributeValue("href", "");

                if (!string.IsNullOrEmpty(href))
                {
                    var fullUrl = new Uri(new Uri(baseUrl), href).ToString();
                    links.Add(fullUrl);
                }
            }
            return links;
        }
        public static string? GetNextPageUrl(string html, string currentUrl)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var next = doc.DocumentNode.SelectSingleNode("//li[@class='next']/a");
            if (next is null) return null;

            var href = next.GetAttributeValue("href", "");
            return new Uri(new Uri(currentUrl), href).ToString();
        }


        public static Book ParseBookPage(string html, string url)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var title = doc.DocumentNode
                .SelectSingleNode("//h1")
                ?.InnerText.Trim() ?? "Unknown";

            var priceText = doc.DocumentNode
                .SelectSingleNode("//p[@class='price_color']")
                ?.InnerText.Trim() ?? "0";

            var ratingNode = doc.DocumentNode
                .SelectSingleNode("//p[contains(@class,'star-rating')]");
            var ratingClass = ratingNode?.GetAttributeValue("class", "") ?? "";

            var description = doc.DocumentNode
                .SelectSingleNode("//div[@id='product_description']/following-sibling::p")
                ?.InnerText.Trim() ?? "No description available.";

            return new Book(
                Title: title,
                Price: Cleaner.CleanPrice(priceText),
                Rating: Cleaner.CleanRating(ratingClass),
                Description: description,
                Url: url);
        }
    }
}
