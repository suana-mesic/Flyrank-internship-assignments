using System.Globalization;

namespace BookScraper.Services
{
    public static class Cleaner
    {
        public static decimal CleanPrice(string raw)
        {
            var digits = new string(raw.Where(c => char.IsDigit(c) || c == '.').ToArray());

            return decimal.TryParse(digits, CultureInfo.InvariantCulture, out var price) ? price : 0m;
        }

        public static int CleanRating(string cssClass)
        {
            if (cssClass.Contains("One")) return 1;
            if (cssClass.Contains("Two")) return 2;
            if (cssClass.Contains("Three")) return 3;
            if (cssClass.Contains("Four")) return 4;
            if (cssClass.Contains("Five")) return 5;
            return 0;
        }
    }
}
