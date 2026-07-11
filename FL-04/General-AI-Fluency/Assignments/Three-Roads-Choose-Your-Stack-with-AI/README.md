# FL-04: Three Roads — Choose Your Stack with AI

**Track:** General AI Fluency | **Week:** 4 | **Phase:** Build
**Intern:** Suana Mešić — Junior Backend Developer

---

## My four constraints

I gave AI my real constraints and made it lay out options with trade-offs instead of picking for me:

1. **Free only** — no paid tools or hosting.
2. **Honest skill level** — beginner at frontend/web hosting, but comfortable with C#, databases, and git from my internship. I must be able to maintain and explain whatever I pick.
3. **What the portfolio needs to do** — a single-page site: Hero → Work → About → Contact, every section pointing to one action (visit my GitHub).
4. **How my work must be shown** — my proof is **code**: a clean code screenshot and a repo link, not image galleries, an embedded demo, or long-form articles.

**Does anything need to be dynamic at launch?** No. The page is static; no contact form or backend needed yet.

---

## The three roads (simplest to most powerful)

### Road 1 — No-code builder (Carrd / Framer)
- **How I'd build:** drag-and-drop in a visual editor, no code.
- **Host (free):** built into the tool's free tier.
- **Backend:** none.
- **Shows my work:** poorly for my case — these tools shine for image-heavy visual portfolios, not code.
- **Trade-off:** fastest to publish, but it isn't "mine" and hides the exact skill (writing and shipping code) I want to prove.

### Road 2 — Plain HTML/CSS on a free host (GitHub Pages)
- **How I'd build:** write the HTML/CSS myself (with AI help), commit to a repo.
- **Host (free):** GitHub Pages, served straight from the repo.
- **Backend:** none needed.
- **Shows my work:** very well — the repo itself is proof I can ship, and it sits right next to my code.
- **Trade-off:** a few more setup steps than drag-and-drop, and I deploy with git — but I already do that daily.

### Road 3 — A framework (React / Next.js)
- **How I'd build:** a full component-based developer setup with a build step.
- **Host (free):** Vercel or Netlify free tier.
- **Backend:** optional, more than I need.
- **Shows my work:** fine, but no better than plain HTML for a four-section static page.
- **Trade-off:** powerful but overkill — I'd spend the build weeks fighting config and build errors instead of showing my work, and it's more to maintain.

---

## Pressure-testing the front-runner (GitHub Pages)

- **What breaks if I pick the simplest (no-code)?** I lose the "repo as proof" advantage, and the tool fights me the moment I want code to be the centerpiece rather than images.
- **What do I maintain if I pick the most powerful (framework)?** A build pipeline, dependencies that go out of date, and config I don't need for a static page — maintenance cost with no matching benefit.
- **Can I finish in two weeks?** Yes, comfortably with Road 2. No build system to debug; I write HTML/CSS and push.
- **Does it show my work the way it needs to be shown?** Yes — code is my proof, and a clean public repo hosting the site is itself evidence I can ship.

---

## Decision and rationale

**I chose Road 2 — plain HTML/CSS on GitHub Pages.**

- **Does it show my work well?** Yes. My proof is code, so a clean repo that also hosts the site makes the site and the proof the same artifact. It also matches my one action — the whole portfolio points to my GitHub, so the site living there is consistent.
- **Can I maintain this?** Yes, and this was the deciding factor. I already use GitHub daily and know `git push` from my deliverables repo, so deploying is something I do anyway. No new tool, nothing that breaks silently.

I did not choose **no-code** because it's built for visual portfolios and hides the code skill I want to prove. I did not choose a **framework** because it's more machinery than a static four-section page needs, and the maintenance cost isn't justified.

**Backend needed at launch?** Honestly, no — not yet. The page is static. Anything dynamic (like a contact form) can come later if it's actually needed; most portfolios need a backend exactly once, later.
