# Break Your Own Site

**Track:** General AI Fluency | **Week:** 9 | **Phase:** Submit
**Intern:** Suana Mešić

---

## Live URL

https://github.com/suana-mesic/suana-mesic.github.io/tree/f7 — The version built specifically for this assignment. You can view it by cloning the repository and double-clicking index.html.
https://suana-mesic.github.io — The live production version, which updates depending on how many tasks I have completed.

---

## What I tried

| Test                                                                        | What happened                                                                                                                                                                                                     |
| --------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Submit form with empty fields                                               | Browser blocked it — `required` attribute on all three fields works                                                                                                                                               |
| Submit form with garbage email ("asdf")                                     | Browser blocked it — `type="email"` validation caught it                                                                                                                                                          |
| Click "Send message" twice fast                                             | First version had a race: button disabled on click, but a fast double-click could fire two fetches before the first disabled the button. Fixed with an `if (btn.disabled) return` guard at the top of the handler |
| Open on phone (real device)                                                 | Already fixed in the "Open It on Your Phone" assignment — nav wraps, padding reduced, touch targets sized                                                                                                         |
| Open in Firefox (had only tested Chrome/Edge)                               | Worked without issues                                                                                                                                                                                             |
| Click every link (LinkedIn, GitHub, CodeWars, CV, Calendly, BookVerse repo) | All working. CV link depends on `Suana_Mesic_CV.pdf` being in the repo — if missing, it 404s silently                                                                                                             |
| Share the URL on LinkedIn/chat                                              | No preview appeared — missing Open Graph meta tags                                                                                                                                                                |
| Check browser tab                                                           | Blank — no favicon                                                                                                                                                                                                |
| Check code block on very narrow screen (~320px)                             | Horizontal scroll appears but there's no visual indicator that you can scroll — the code just looks cut off                                                                                                       |
| Run speed check                                                             | Google Fonts load externally — on slow connections the page renders with fallback fonts first, then shifts when Fraunces/Inter load (FOUT)                                                                        |
| Search "Suana Mešić portfolio" on Google                                    | Not indexed yet — no `robots.txt` or `sitemap.xml`, but GitHub Pages allows crawling by default so it will index eventually                                                                                       |

---

## Triage

### Fix-now (done)

**1. No Open Graph meta tags.**
Added `og:title`, `og:description`, `og:type`, `og:url`, and `twitter:card` tags. Now when someone shares the link on LinkedIn or in a chat, a proper preview appears with title and description.

**2. No favicon.**
Created an "S" monogram in the identity kit's burgundy (#6E2A46) — the same one designed in FL-03. Added `<link rel="icon" href="favicon.ico">` to the head.

**3. Double-submit on fast click.**
Added `if (btn.disabled) return;` as the first line of the submit handler. Now the second click is ignored entirely, even if it fires before the fetch starts.

### Known limitations (not fixed)

**Code block overflow on very narrow screens.** The `<pre>` block scrolls horizontally on screens under ~350px, but there's no scrollbar indicator on mobile. The code just looks cut off. Fixing this properly would mean either wrapping long lines (which breaks code readability) or adding a visible scroll hint. Not worth the trade-off for a portfolio that few people will view at 320px.

**Google Fonts flash (FOUT).** Fraunces and Inter load from Google's CDN. On slow connections, text renders in the system font first and shifts when the web fonts arrive. Self-hosting the fonts would fix it but adds complexity. The flash is brief and the fallback fonts are readable.

**No `robots.txt` or `sitemap.xml`.** GitHub Pages allows indexing by default, so Google will find the site eventually without these. Adding them would speed up indexing slightly but isn't blocking.

**CV link breaks if the PDF isn't in the repo.** The link points to `Suana_Mesic_CV.pdf` as a relative path. If someone clones the repo and doesn't include the PDF, or if the filename changes, the link silently 404s. A proper fix would be hosting the CV externally (Google Drive, LinkedIn) and linking there.

---

## Files

- `README.md` — this document
