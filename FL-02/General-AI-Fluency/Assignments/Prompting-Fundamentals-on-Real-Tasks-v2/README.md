# FL-02: Prompting Fundamentals on Real Tasks — Prompt Iteration Log

**Track:** General AI Fluency | **Week:** 2 | **Phase:** Foundations
**Intern:** Suana Mešić — Junior Backend Developer

---

## The task

I took one real FL-01 task — writing code for BookVerse — and started from the naive prompt I'd actually type, then iterated through five versions, each adding one named technique. Final prompt run on both Claude and ChatGPT.

**Coding task:** a C# background service that notifies staff when a customer pays — something I built in BookVerse, so I can judge the output.

---

## The iteration log

### Baseline — naive prompt
> Write a background service in C# that sends notifications.

**Output:** a generic timer-based service emailing every 15 minutes. It invented a scenario because I gave it none.

### V1 — ROLE ASSIGNMENT
> **You are a senior .NET backend developer.** [...]

**Output difference:** code got more production-shaped — added a `Channel<T>` queue, retry with backoff, graceful shutdown. Still invented the scenario.

### V2 — CONTEXT & MOTIVATION
> [...] **I'm building an e-commerce backend. When a customer pays, staff need notifying, but the notification must not roll back the payment if it fails.** [...]

**Output difference:** biggest jump. It recognised the problem and chose the **Transactional Outbox pattern** on its own. Giving it the *why* changed which architecture it picked.

### V3 — OUTPUT STRUCTURE
> [...] **Structure: (1) approach, (2) code, (3) DI, (4) assumptions.** [...]

**Output difference:** same content, now reviewable. The "assumptions" section exposed what it had guessed — most useful part.

### V4 — STEP DECOMPOSITION
> [...] **Before coding, reason step by step: handoff, consumption, failure isolation.** [...]

**Output difference:** code nearly identical to V3, but the reasoning it wrote first explained *why* (outbox insert can't fail independently; singleton needs a fresh DI scope). Helped my understanding more than the code.

### V5 — FEW-SHOT EXAMPLE
> [...] **Here is the interface style I use:** `IPaidOrderNotificationQueue` [...]

**Output difference:** tightest, most usable. It wrote exactly the consumer I asked for, matched my `IAsyncEnumerable` style and SignalR delivery, and placed the try/catch correctly inside the loop. Stopped it inventing its own structure.

---

## Cross-model comparison (final prompt V5)

Both **Claude** and **ChatGPT** produced a correct `await foreach` consumer with per-notification try/catch and correct failure isolation.

**Specific difference:** Claude sent to a `"staff"` SignalR group and wired the hub + `MapHub` endpoint, reasoning about *who* receives the message. ChatGPT sent to `Clients.User(userId)` — cleaner but a per-user assumption that didn't quite match "notify all staff."

**Verdict:** both shippable; Claude fit my "notify staff" intent more precisely because it reasoned about the audience.

---

## Final reusable template

> You are a senior [STACK] developer. I'm building [PROJECT CONTEXT]. I need [WHAT IT MUST DO], and it must [KEY CONSTRAINT / WHY].
> Here is the style I use: `[SMALL CODE EXAMPLE]`
> Write [THE THING]. Structure: (1) approach, (2) code, (3) registration, (4) assumptions.

The two techniques that mattered most: **context + motivation** (picked the right architecture) and **few-shot example** (matched my codebase instead of inventing one).
