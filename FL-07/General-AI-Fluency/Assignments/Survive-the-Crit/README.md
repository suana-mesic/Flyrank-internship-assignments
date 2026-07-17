# Survive the Crit

**Track:** General AI Fluency | **Week:** 7 | **Phase:** Build+
**Intern:** Suana Mešić

---

## Live URL

- **https://github.com/suana-mesic/suana-mesic.github.io/tree/f6** — The version built specifically for this assignment. You can view it by cloning the repository and double-clicking index.html.
- **https://suana-mesic.github.io** — The live production version, which updates depending on how many tasks I have completed.

---

## Proof statement

"I turn messy requirements into clean, reliable backend code."

---

## Reviewer feedback

### Reviewer 1 — a colleague from my faculty (same field)

**In ten seconds, what do I do?**
"You're a backend developer who builds excellent and scalable applications."

**Would you believe I'm good at it?**
"Yes — not just from what's on the portfolio, but also based on your academic results."

**Other feedback (from an earlier session):**
"The design is good and the text reads well, but you're worth a lot more than what the portfolio shows."

### Reviewer 2 — a friend (different perspective)

**In ten seconds, what do I do?**
"You're a backend developer who develops excellent and scalable applications without errors."

**Would you believe I'm good at it?**
"Yes — not only based on what's presented here but also based on your academic results."

---

## Sort

### Must-fix

**1. Contact form doesn't clear after submission.** After clicking "Send message", the form submits successfully but the fields stay filled with the same text. If the visitor comes back to the page (via browser back), it looks like nothing happened and they might submit again. The form should clear on successful submission.

**2. Code screenshot is blurry.** Reviewer 2 pointed out that the code screenshot is hard to read and suggested embedding the actual code with formatting and syntax highlighting instead of an image. A screenshot can't be copied, searched, or read by a screen reader either.

### Nice-to-have

- **Only one project shown.** Reviewer 1 said the portfolio under-represents me. I have a finished capstone that is arguably stronger than BookVerse — but I built it with heavy AI assistance and I'm not confident I can explain every piece of it yet. The assignment says "you must understand every file you deploy", so I'm keeping it off the site until I can genuinely own it.
- **No favicon.** The "S" monogram from my identity kit isn't wired up. The browser tab is blank.
- **No live demo or screen recording.** BookVerse isn't deployed; the only way to see it run is to clone it.
- **Academic results not mentioned.** Both reviewers referenced my grades unprompted, which means they matter to people evaluating me — but the portfolio says nothing about them.

---

## What I fixed

**1. Form now clears after submission.** Added JavaScript that intercepts the form submit, sends it via fetch instead of a full page redirect, shows a "Thank you!" message, and resets all fields. The visitor stays on the page and sees confirmation without the form looking stale.

**2. Replaced the blurry code screenshot with a formatted code block.** The actual C# code from `PaidOrderNotificationBackgroundService.cs` is now embedded with syntax highlighting — keywords in purple, types in teal, strings in orange, comments in green. It's the real code, readable on any screen size, and can be copied.

---

## Files

- `README.md` — this document
