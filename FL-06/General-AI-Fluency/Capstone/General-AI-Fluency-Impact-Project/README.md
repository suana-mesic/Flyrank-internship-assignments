# General AI Fluency — Impact Project
**Capstone — General AI Fluency | Week 6**
**Intern:** Suana Mešić

The Impact Project is the one real thing that proves what I can do: a personal brand that lives on the internet, and a personal agent that actually runs. Not a tutorial, not a mock — a site anyone can open and an agent anyone can talk to. This capstone ties together the pieces I built across the General AI Fluency track (identity kit, deployment, prompting, MCP basics, agent design and build) into a single story about using the AI stack to ship something that is mine.

Three deliverables: a live personal brand website, a shipped personal agent, and the AI-stack fluency that produced both.

---

## 1. Personal brand website — live

**Live:** https://suana-mesic.github.io
**Source:** https://github.com/suana-mesic/suana-mesic.github.io

One page, plain HTML and CSS, no framework. It carries my positioning (who I am and what I do), a BookVerse case study written in the three-beat shape (problem → what I did → what came of it), and working links to GitHub, my CV (PDF), and a booking link. Hosted on GitHub Pages; HTTPS is automatic (GitHub provisions a Let's Encrypt certificate), so the site loads securely with no manual setup.

I deliberately kept it a single hand-written page rather than reaching for a framework — the point of this site is that I can explain every line of it, and that it loads instantly and works on a phone. A FlyRank subdomain (`suana.flyrank.ai`, via a CNAME to the GitHub Pages site) is the planned custom-domain step; the site itself doesn't change when it's pointed there — the custom domain is a pointer, not a migration.

Evidence: `evidence/website-desktop.png`, `evidence/website-mobile.jpg` (same site on a phone), `evidence/identity-kit.png` (the brand decisions the site is built on).

---

## 2. Personal agent — shipped

**Lexor Leave Advisor** — a Claude Project that reads a CSV of employee leave records and answers HR questions about leave patterns, trends, and predictions.

**Demo video (≈4 min, unlisted):** https://youtu.be/hlKMiQiBpqE

It sits beside Lexor, the HR app my team built for Software Development II at FIT Mostar: it takes the same data the app stores (employees, departments, leave types, leave records) and turns it into answers a manager can act on — who is absent, how often, and what is likely next — from a single CSV, without touching any production database.

The agent's value is in its rules, not in infrastructure. It bases every answer on the data and refuses to invent numbers; it hedges predictions instead of stating them as fact; and it has hard safety guardrails — it will not recommend firing or disciplining anyone from absence data, and it will not diagnose a medical condition from a sick-leave pattern. All five eval cases from the design phase pass on the final agent, including the two that matter most: refusing to calculate a cost it has no salary data for, and refusing a "should we fire her?" question while offering to show the actual records instead.

Evidence: `evidence/agent-demo-1.png` (a real answer with a table and stated date range), `evidence/agent-demo-safety.png` (the safety guardrail refusing a termination question).

---

## 3. Mastering the AI stack

Both deliverables were built by using AI across the whole stack rather than as a single chat window:

- **Identity and positioning** — decided once as an identity kit, then reused everywhere (site copy, CV, agent voice).
- **Prompting** — moved from one-shot asks to a prompt ladder on real tasks, which is what made the agent's instructions tight enough to pass its evals.
- **Deployment** — shipped a live, HTTPS site on GitHub Pages and understood the DNS chain end to end (resolver → root → TLD → GitHub, plus what a CNAME does for the custom domain).
- **Agents and MCP** — learned agent concepts and MCP basics, then designed, built, and evaluated a real agent against five written cases with pass/fail criteria.

The through-line: pick the smallest tool that does the job. The site is plain HTML because it doesn't need more; the agent is a Claude Project (not an n8n workflow or a paid custom GPT) because its only input is a file and its only output is text.

---

## How to reproduce the agent

A stranger can rebuild Lexor Leave Advisor in under ten minutes: create a Claude Project named "Lexor Leave Advisor", paste the instruction block (rules + safety guardrails), upload `lexor-leaves.csv`, and start asking questions in plain language. The full instructions, setup steps, usage examples, and eval results are documented in `FL-09/General-AI-Fluency/Assignments/Documentation-and-Demo-Video/`.

---

## What it proves

I can take an idea from positioning to a live, secure website; I can design an agent around a real job, give it honest guardrails, and prove it works with written evals; and I can choose the right tool at each step instead of the biggest one. The site is where the next case study goes — adding one is a short conversation with my Claude Project, not a rebuild.

---

## Files

```
General-AI-Fluency-Impact-Project/
├─ README.md                       this document
└─ evidence/
   ├─ website-desktop.png          the live portfolio, desktop
   ├─ website-mobile.jpg           the same site on a phone
   ├─ identity-kit.png             brand decisions behind the site
   ├─ agent-demo-1.png             Lexor answering from real data
   └─ agent-demo-safety.png        Lexor refusing a termination question
```

Related work across the track: `FL-05` (website + DNS), `FL-06` (agent design), `FL-07` (agent build + phone check), `FL-08` (agent does something), `FL-09` (documentation + demo video).
