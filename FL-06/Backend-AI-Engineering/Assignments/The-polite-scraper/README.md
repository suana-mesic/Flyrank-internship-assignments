# The Polite Scraper

**Track:** Backend AI Engineering | **Week:** 6 | **Phase:** Build
**Intern:** Suana Mešić

---

## What it does

A .NET console app that scrapes books.toscrape.com — a practice site built for learning web scraping. It collects 1,000 books with title, price, rating, description, and URL, then saves them as structured JSON. The output becomes the RAG corpus for next week.

---

## The "polite" part

Three things make this scraper one a site owner would allow:

1. **robots.txt** — the program reads it before scraping and stops if the path is disallowed.
2. **Rate limiting** — 1-second delay between every request. 1,000 books take ~20 minutes instead of ~10 seconds, on purpose.
3. **User-Agent** — every request identifies itself: `BookScraper/1.0 student-project suana.mesic@edu.fit.ba`. No pretending to be a browser.

---

## Pipeline

```
robots.txt check
      │ allowed?
      ▼
catalogue page-1 ──► extract 20 book links ──► next page?
      │                      │                     │
      │                      ▼                     ▼
      │               open each book         page-2, page-3 ...
      │               extract fields              (50 pages)
      │                      │
      │                      ▼
      │               clean data
      │               "£51.77" → 51.77
      │               "Three" → 3
      │                      │
      │                      ▼
      └──────────► output/books.json (1000 books)
```

---

## Data cleaning

| Raw value | Cleaned | How |
|---|---|---|
| `£51.77` | `51.77` | Strip non-numeric characters, parse as decimal |
| `star-rating Three` | `3` | Map CSS class word to integer |
| HTML entities | Clean text | HtmlAgilityPack handles decoding |

---

## Run it

```bash
cd BookScraper
dotnet run
```

Output appears in `output/books.json`. For 1,000 books expect ~20 minutes (rate limiting).

To scrape fewer books for testing, change `maxPages` in `Program.cs` (3 pages = 60 books, ~1 minute).

---

## Sample output

```json
{
  "Title": "A Light in the Attic",
  "Price": 51.77,
  "Rating": 3,
  "Description": "It's hard to imagine a world without A Light in the Attic...",
  "Url": "https://books.toscrape.com/catalogue/a-light-in-the-attic_1000/index.html"
}
```

---

## Files

```
The-polite-scraper/
├─ BookScraper/
│  ├─ Models/Book.cs              the record: title, price, rating, description, url
│  ├─ Services/
│  │  ├─ RobotsChecker.cs         reads robots.txt, checks if path is allowed
│  │  ├─ Fetcher.cs               downloads a page with User-Agent and 1s delay
│  │  ├─ Parser.cs                extracts book links, next page, and book fields
│  │  └─ Cleaner.cs               price string → decimal, rating word → int
│  ├─ Program.cs                  main loop: pages → links → books → JSON
│  └─ output/books.json           1,000 scraped books
├─ BookScraper.sln
└─ .gitignore
```
