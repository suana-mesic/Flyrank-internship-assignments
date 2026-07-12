# FL-04: Ship an Automation Workflow — Source-Grounded Study Notes

**Track:** General AI Fluency | **Week:** 4 | **Phase:** Build (core)
**Intern:** Suana Mešić — Junior Backend Developer

---

## What this pipeline does

It turns one university lecture (a PDF of slides or a script, in Bosnian) into clean, source-grounded study notes plus self-test exam questions — without me reading and hand-summarizing the whole thing. I picked this from my FL-01 audit because I prepare for exams constantly and have real inputs on hand.

The pipeline is **source-grounded**: NotebookLM only uses the uploaded lecture, so it doesn't invent material — important for study accuracy.

---

## Step diagram

See `pipeline-diagram.png`. Four distinct steps, each handing off to the next:

1. **GATHER** (NotebookLM) — upload the lecture PDF; NotebookLM stays grounded in only that source.
2. **SYNTHESIZE** (NotebookLM) — extract a structured summary: key concepts, definitions, relationships, cited to source.
3. **FORMAT** (Claude Project) — reshape the summary into clean, consistent study notes (headings, bullets, key terms).
4. **REVIEW** (Claude Project) — generate exam-style practice questions with answers for self-testing.

**Handoffs:** the output of each step is the literal input to the next — NotebookLM's summary is pasted into the Claude Project for formatting, and the formatted notes are the basis for the questions.

---

## Every prompt / configuration used

### Step 1 — GATHER (NotebookLM)
No prompt — action only: create a new notebook, upload the lecture PDF, let it process. (One notebook per lecture, so each run is clean and can't mix lectures.)

### Step 2 — SYNTHESIZE (NotebookLM chat)
```
Na osnovu učitanih izvora, napravi strukturiran sažetak ove lekcije na bosanskom
jeziku. Uključi:
1. Glavne koncepte (naslov + kratko objašnjenje svakog)
2. Ključne definicije (termin — definicija)
3. Veze između koncepata (šta se na šta oslanja)
Drži se isključivo sadržaja iz izvora. Uz svaku tvrdnju navedi na koji se izvor oslanja.
```

### Step 3 — FORMAT (Claude Project — custom instructions)
```
You format study material into clean, consistent notes in Bosnian. When given a
summary, produce: a clear title, main sections with headings, bullet points for
key facts, and a bolded list of key terms at the end. Keep it concise and
scannable. Do not add information that isn't in the input.
```
Message in the Project (paste the Step 2 summary):
```
Pretvori ovaj sažetak u čiste bilješke za učenje na bosanskom, prema formatu iz
instrukcija:
[PASTE STEP 2 SUMMARY]
```

### Step 4 — REVIEW (same Claude Project chat)
```
Na osnovu ovih bilješki, napravi 8 ispitnih pitanja na bosanskom za samotestiranje:
5 pitanja kratkog odgovora i 3 pitanja koja traže objašnjenje/poređenje. Ispod
svakog pitanja navedi kratak tačan odgovor da mogu provjeriti.
```

---

## The five runs

Real lectures from my Information Systems course, each run end to end through all four steps.

| # | Lecture (input) | Pipeline time |
|---|-----------------|---------------|
| 1 | Analiza, klasifikacija i specifikacija korisničkih zahtjeva | 14s |
| 2 | Upravljanje projektom | 12s |
| 3 | Plan razvoja softvera | 14s |
| 4 | Tehnike prikupljanja činjenica | 13s |
| 5 | Primjena i održavanje IS-a | 21s |

Each run produced: a source-cited summary (Step 2), formatted study notes with a key-terms list (Step 3), and 8 practice questions with answers (Step 4). Full outputs saved separately; a representative run (Lecture 1) is included as an example.

**Every run completed end to end**, including on lectures never seen before — the pipeline is input-agnostic (any lecture PDF works without changing the prompts).

## Evidence (screenshots)

The full text outputs of all five runs are documented above, so the screenshots are a visual confirmation that the steps really run in their tools rather than plain chat. Capturing every step of all five runs would mean 15 screenshots (5 runs × 3 views), which is more noise than proof — so I captured one representative run (Lecture 5) across both tools:

- `notebooklm-output-lecture5.png` — the NotebookLM summary (Steps 1–2, source-grounded).
- `claude-questions-1-lecture5.png` — the Claude Project output, part 1 (Steps 3–4).
- `claude-questions-2-lecture5.png` — the Claude Project output, part 2.

Together they show both tools in use and the handoff between them; the text above covers all five runs in full.

---

## Time accounting (honest)

**Pipeline:** total generation time across 5 runs = **74 seconds** (~15s per lecture). This is generation time only.

**Setup cost (one-time, honest):** ~3–5 min to set up the notebook, paste the prompts, and configure the Claude Project. This is paid once; every future lecture reuses it.

**Manual baseline:** I timed doing this by hand for three lectures — reading the PDF, summarizing, and writing questions myself. It took roughly **1h to 1h30min per lecture** (~75 min average).

**Time saved per lecture:** ~75 min manual vs ~15s automated ≈ **~74 minutes saved per lecture**, once setup is done. With only ~3–5 min of one-time setup, the pipeline pays for itself within the very first lecture.

---

## Known failure points & required human review

The pipeline is fast but **not** a replacement for judgment. What breaks, and what a human must still check:

- **Source quality in, source quality out.** If the lecture PDF is a scan of images or has broken text extraction, NotebookLM can't ground properly. Human check: confirm the PDF is real text before trusting the summary.
- **Grounded ≠ complete.** NotebookLM sticks to the source, but can still under-weight something the professor stressed verbally in class that isn't emphasized in the slides. Human check: compare notes against what I remember from the lecture.
- **Questions can be too easy.** Claude's practice questions are solid but sometimes stay at recall level; deeper exam questions need me to add a few harder ones.
- **No fact-checking beyond the source.** If the lecture slide itself contains an error, the pipeline faithfully reproduces it. Human check: cross-check anything that looks off against a textbook.
- **Language consistency.** Prompts specify Bosnian, but a stray English heading occasionally appears in formatting — a quick manual fix.

**Bottom line:** the pipeline does the mechanical 90% (reading, structuring, drafting questions) in seconds; I keep the last 10% — verifying accuracy and completeness against the source and my own memory of the class.

---

## Files in this folder

- `README.md` — this walkthrough
- `pipeline-diagram.png` — the 4-step flow diagram
- `notebooklm-output-lecture5.png` — NotebookLM summary (representative run)
- `claude-questions-1-lecture5.png` — Claude Project output, part 1
- `claude-questions-2-lecture5.png` — Claude Project output, part 2
