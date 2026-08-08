# Plant Your Flag: Domain + Badge

**Track:** General AI Fluency | **Week:** 9 | **Phase:** Submit
**Intern:** Suana Mešić

---

## Live URL

**https://github.com/suana-mesic/suana-mesic.github.io/tree/f8** — The version built specifically for this assignment. You can view it by cloning the repository and double-clicking index.html.

**https://suana-mesic.github.io** — The live production version, which updates depending on how many tasks I have completed.

---

## Domain

The site is live over **HTTPS** on a clean free subdomain — `suana-mesic.github.io` (GitHub Pages, with a Let's Encrypt certificate provisioned automatically). Per this assignment's Q&A, any free subdomain qualifies, so this is the accepted zero-budget path. When my FlyRank subdomain (`suana.flyrank.ai`) is provisioned at capstone approval, I add it as a custom domain in the GitHub Pages settings — a pointer, not a rebuild, and both URLs keep working.

---

## Analytics

**GoatCounter** — free, privacy-friendly (no cookies, no consent banner), added as one script tag before `</body>`:

```html
<script data-goatcounter="https://suana-mesic.goatcounter.com/count"
        async src="//gc.zgo.at/count.js"></script>
```

Confirmed working: after deploying, visiting the live site registered a pageview in the dashboard — see `goatcounter-analytics.png` (1 visit on `/` "Suana Mešić — Backend Developer", Chrome · Windows · Bosnia and Herzegovina).

---

## Launch hygiene

- **Social-share preview:** Open Graph + Twitter Card tags in `<head>`, with a 1200×630 share image (`og-image.png`) in my identity-kit colours; `twitter:card` is `summary_large_image`. Verified on opengraph.xyz — see `share-preview.png`.
- **Favicon:** `favicon.ico` (my "S" monogram) shows in the browser tab.
- **Page title:** `Suana Mešić — Backend Developer`, with a matching meta description.
- Re-checked the final address once more on a real phone — see `phone.png`.

---

## FlyRank graduate badge

Pending. The graduate badge and verification page become available after the main-track requirements (5 assignments + capstone) are approved (per this assignment's Q&A, around the completion review). The footer already has the slot wired and commented out; I uncomment it and drop in the real badge image + verification link the moment the certificate is issued.

---

## Files

- `README.md` — this document
- `goatcounter-analytics.png` — analytics installed and recording a live pageview
- `share-preview.png` — social-share card rendering correctly (opengraph.xyz)
- `phone.png` — the live site on a real phone
