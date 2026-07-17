# Open It on Your Phone

**Track:** General AI Fluency | **Week:** 7 | **Phase:** Build+
**Intern:** Suana Mešić

---

## Live URL

**https://github.com/suana-mesic/suana-mesic.github.io/tree/f4** — The version built specifically for this assignment. You can view it by cloning the repository and double-clicking index.html.

**https://suana-mesic.github.io** — The live production version, which updates depending on how many tasks I have completed.

---

## Fix log

### 1. Nav overflow on narrow screens

**Before:** nav links (Work, About, Contact, GitHub button) were in a single flex row with no wrapping. On phones under ~480px, they either overflowed or got squeezed together.
**Fix:** added `@media (max-width: 30rem)` that wraps the nav, reduces gap and font size, and shrinks the button padding. Nav now fits on any phone.

### 2. Nav links too small to tap

**Before:** nav links had no vertical padding — just text. Touch target was the line-height of the text, well under the recommended 44px minimum.
**Fix:** added `padding: 0.5rem 0` to `.nav-links a`, making the tappable area tall enough for a finger.

### 3. Too much whitespace on mobile

**Before:** hero padding was `6rem 0 5rem` and section padding was `5rem 0`. On a phone screen that's half the viewport just white space before you see content.
**Fix:** reduced to `3.5rem 0 3rem` for hero and `3rem 0` for sections below `30rem`. Desktop stays the same.

### 4. No horizontal scroll protection

**Before:** no `overflow-x: hidden` on `<html>`. If any element even slightly overflowed (e.g. a wide image), the phone would show a horizontal scrollbar and the whole page would shift.
**Fix:** added `overflow-x: hidden` to `html`.

### 5. Figcaption too small on mobile

**Before:** `0.85rem` — readable on desktop, hard to read at arm's length on a phone.
**Fix:** bumped to `0.9rem`.

### 6. Images had no max-height

**Before:** a very tall screenshot (like a full-page code capture) would take over the entire mobile viewport with no way to scroll past it quickly.
**Fix:** added `max-height: 600px; object-fit: contain` to `figure img`. Image stays visible but doesn't dominate the page.

---

## Checks done

- Opened on a real phone (not just a resized browser)
- All nav links work and scroll to the right section
- GitHub, LinkedIn, CodeWars, CV, and Calendly links all open correctly
- BookVerse repo link works
- Images load and don't spill outside their container
- Text is readable at phone distance
- No horizontal scroll on any width

---

## Files

- `README.md` — this fix log
- `portfolio-on-phone.jpg` — screenshot from a real phone after fixes
