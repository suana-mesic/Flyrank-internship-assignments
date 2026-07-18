namespace BookScraper.Models
{
    public sealed record Book(
       string Title,
       decimal Price,
       int Rating,
       string Description,
       string Url);
}
