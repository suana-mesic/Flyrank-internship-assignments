using BookScraper.Models;
using BookScraper.Services;
using System.Text.Json;

var baseUrl = "https://books.toscrape.com/";
var startUrl = "https://books.toscrape.com/catalogue/page-1.html";
var maxPages = 50;

var http = new HttpClient();
http.DefaultRequestHeaders.Add("User-Agent", "BookScraper/1.0 student-project");

if (!await RobotsChecker.IsAllowedAsync(http, baseUrl, "/catalogue/"))
{
    Console.WriteLine("Scraping is not allowed by robots.txt. Exiting.");
    return;
}

var bookLinks = new List<string>();
var currentUrl = startUrl;
var page = 0;

while (currentUrl is not null && page < maxPages)
{
    page++;
    Console.WriteLine($"\n--- Page {page} ---");

    var html = await Fetcher.GetPageAsync(http, currentUrl);
    var links = Parser.GetBookLinks(html, currentUrl);
    bookLinks.AddRange(links);

    Console.WriteLine($"Found {links.Count} books on this page");

    currentUrl = Parser.GetNextPageUrl(html, currentUrl);
}

Console.WriteLine($"\nTotal book links: {bookLinks.Count}");

var books = new List<Book>();

foreach (var link in bookLinks)
{
    try
    {
        var html = await Fetcher.GetPageAsync(http, link);
        var book = Parser.ParseBookPage(html, link);
        books.Add(book);
        Console.WriteLine($"  ✓ {book.Title} — £{book.Price} — {book.Rating}/5");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ✗ Failed: {link} — {ex.Message}");
    }
}

Directory.CreateDirectory("output");
var jsonPath = Path.Combine("output", "books.json");

var options = new JsonSerializerOptions { WriteIndented = true };
var json = JsonSerializer.Serialize(books, options);
await File.WriteAllTextAsync(jsonPath, json);

Console.WriteLine($"\nDone! {books.Count} books saved to {jsonPath}.");