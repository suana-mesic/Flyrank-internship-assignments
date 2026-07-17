# FL-05: Ship the Ugly One

**Track:** General AI Fluency | **Week:** 5 | **Phase:** Build
**Intern:** Suana Mešić

---

## Live URL

**https://github.com/suana-mesic/suana-mesic.github.io/tree/f2** — The version built specifically for this assignment. You can view it by cloning the repository and double-clicking index.html.

**https://suana-mesic.github.io** — The live production version, which updates depending on how many tasks I have completed.

Every section from my sitemap is live and reachable: Hero → Work → About → Contact. The nav works, the case opens, and every call to action points at the one action from Week 1 — visit my GitHub.

Three weeks ago this URL said "Portfolio coming soon". Now it has the real work in it: my actual BookVerse case, a real screenshot of my own code, my architecture diagram, my photo, my identity kit.

---

## How it's built

Plain HTML and CSS in a single file, hosted on GitHub Pages from the `suana-mesic.github.io` repo. No framework, no build step, no JavaScript.

- **Structure:** one `<nav>` and four `<section>` elements with ids (`#hero`, `#work`, `#about`, `#contact`). The nav links are anchors to those ids — that's the whole navigation, no script needed.
- **Colours and type:** my identity kit lives in `:root` as CSS variables, and every rule uses `var(--main)` and friends. Changing one line changes the whole page.
- **The case layout:** three columns for problem → what I did → result, the structure from my FL-02 case study. The middle one has the accent border because it's the part I actually did.
- **Responsive:** one `@media (min-width: 48rem)` breakpoint. Below it, the three columns stack.
- **The one deliberate visual choice:** the code screenshot is the only dark, dense thing on the page. Everything else is quiet. That's my identity kit's style note made literal — a calm frame so the code stays the loudest thing on the page.

Nothing on this page is mystery code. The one thing I had to work out while building it was a CSS specificity collision: `.nav-links a:hover` set the text colour and `.btn:hover` set the background, both to the accent, so the GitHub button's label vanished on hover — rose text on a rose background. `.nav-links a:not(.btn):hover` fixed it. Two classes beat one, and I hadn't thought about which rule would win.

---

## The one real person

I sent the link to a colleague from my faculty — someone in the same field who'd know what they were looking at.

**What they saw:** the design. They said it looks good and read it through without stumbling.

**What confused them:** nothing. No objection to the text, no question about what any section meant.

**Whether the work landed:** partly — and this is the useful part. Their reaction was that I'm worth a lot more than what the portfolio shows.

That's not a compliment I get to enjoy. It's a finding. The page reads well and is clear about _one_ project, and someone who already knows me immediately noticed the gap between what's on the page and what I've actually built. A recruiter who doesn't know me wouldn't notice — they'd just see less. Which means the site's problem isn't the design or the writing. It's that it under-represents me, and I'm the only one who can fix that by putting more of the work in.

---

## Still ugly

Things I already know are rough:

- **Only one case.** BookVerse is there; my capstone (a widget and lead-capture platform — public endpoint hardening, CORS, rate limiting, geo fallback chain, 10 tests) isn't, and neither is the RFID project. This is exactly what my colleague reacted to. The capstone is finished, public and arguably stronger than BookVerse. It should be on the page.
- **No favicon.** I designed an "S" monogram in my identity kit and never wired it up. The browser tab is blank — the one place my logo was supposed to live.
- **No live demo.** BookVerse isn't deployed anywhere. The only way to see it run is to clone it, which almost nobody will do. A short screen recording would close that gap.
- **The code screenshot is an image.** It can't be copied, searched, or read properly by a screen reader — only the alt text describes it.
- **No LinkedIn link** in Contact, even though my profile is set up and my CV is on it.
- **Nothing about the rest of me.** No mention of my grades, awards, tutoring, or CodeWars. Some of that belongs on a portfolio and some doesn't, but I haven't actually decided which — I just left it all out.
- **Not verified on a real phone yet.** It's responsive in the browser at a narrow width, which isn't the same thing.

The honest summary: the frame is done and the frame is fine. What's inside it is one seventh of what I have.

---

## Files

- `README.md` — this note
- `portfolio-at-submission.png` — the page as it looked when I submitted, so the "still ugly" list has something to point at
