# FL-02: Prompting Fundamentals on Real Tasks — Prompt Iteration Log

**Track:** General AI Fluency
**Week:** 2 | **Phase:** Foundations
**Intern:** Suana Mešić — Junior Backend Developer

---

## The task

Take one real FL-01 target task — **writing code for my projects (BookVerse)** — and start from the naive prompt I would actually have typed. Then iterate through five more versions, each adding exactly one named prompting technique, saving every output. Finally, run the last prompt on both Claude and ChatGPT and compare honestly.

**Concrete coding task:** a background service in C# that sends staff notifications when a customer pays for an order — something I actually built in BookVerse, so I can judge output quality.

---

## The iteration log

### Baseline — naive prompt

> Write a background service in C# that sends notifications. Show the code directly in your response, do not create files.

**Output (excerpt):** A generic Windows Service with a `Timer`-based loop that fires an email every 15 minutes. Pluggable `INotificationSender`, but no connection to any real trigger — it just sends on a schedule.

**Note:** This is a real answer to a vague question. It invented a scenario (scheduled emails) because I gave it none. Nothing about payments, orders, or my actual problem.

---

### Version 1 — technique: ROLE ASSIGNMENT

> **You are a senior .NET backend developer.** Write a background service in C# that sends notifications. [...]

**What I added:** a role/persona at the start.

**Observed output difference:** the code got noticeably more production-shaped — it added a `Channel<T>`-based in-memory queue, exponential-backoff retry with `MaxAttempts`, and a `StopAsync` override. The role pushed it from "example code" toward "code a senior would actually ship." But it still invented the scenario; it didn't know what I was building.

---

### Version 2 — technique: CONTEXT & MOTIVATION

> You are a senior .NET backend developer. **I'm building an ASP.NET Core e-commerce backend. When a customer pays for an order, staff need to be notified. The notification must not run inside the payment transaction, because if it fails it must not roll back the payment.** [...]

**What I added:** the real context and the *why* (a failed notification must not roll back the payment).

**Observed output difference:** this was the biggest jump. The model switched entirely — it recognised the problem and reached for the **Transactional Outbox pattern** on its own, wrote an `OutboxMessage` entity, a `PaymentService` that writes the outbox row inside the payment transaction, and a polling `BackgroundService` outside it. It even explained *why* (durability across crashes). Giving it the motivation, not just the task, changed which architecture it chose.

---

### Version 3 — technique: OUTPUT STRUCTURE

> [...same context...] **Structure your answer as: (1) a short explanation of the approach, (2) the full code, (3) the DI registration line, (4) any assumptions you made.** [...]

**What I added:** a required output structure (four numbered sections).

**Observed output difference:** the same strong content, but now scannable and reviewable. The "assumptions" section was the most useful addition — it surfaced things it had guessed (that `AppDbContext` has an `OutboxMessages` set, that roles are Employee/Manager/Admin), which I'd otherwise have had to hunt for in the code. Structure didn't improve the code, it improved my ability to check it.

---

### Version 4 — technique: STEP DECOMPOSITION

> [...same context and structure...] **Before writing code, reason through it step by step: first decide how the payment flow hands off the notification, then how the background service consumes it, then how a failure is isolated.** [...]

**What I added:** explicit step-by-step reasoning before the code.

**Observed output difference:** the code was almost identical to V3 — but the reasoning it wrote first was genuinely valuable. It explained *why* the outbox insert can't fail independently (it's just a DB row, no network call), and *why* `BackgroundService` needs a fresh DI scope per iteration (it's a singleton, so a captured scoped `DbContext` would be a bug). This is honest: step decomposition didn't change the output much, it exposed the reasoning behind it — useful for me learning, less for the final code.

---

### Version 5 — technique: FEW-SHOT EXAMPLE (constrain by example)

> [...] **Here is the interface style I use:**
> ```csharp
> public interface IPaidOrderNotificationQueue
> {
>     IAsyncEnumerable<PaidOrderNotification> DequeueAllAsync(CancellationToken ct);
> }
> ```
> Write the background service that consumes this queue. [...]

**What I added:** a concrete example of my own interface style, constraining the shape of the answer.

**Observed output difference:** this was the tightest, most usable output. Instead of re-deriving the whole outbox stack, it wrote exactly the consumer I asked for — an `await foreach` over my interface, with the try/catch correctly placed *inside* the loop (it even explained that wrapping the whole enumeration would fail to isolate a single bad send). It matched my `IAsyncEnumerable` style and my SignalR delivery. Giving it my example stopped it from inventing its own structure and made it fit my actual codebase.

---

## Cross-model comparison (final prompt: V5)

I ran the final prompt on both **Claude (Sonnet)** and **ChatGPT**.

**Structure:** Both produced a correct `await foreach` consumer with per-notification try/catch, both isolated failures correctly, both matched my interface. On the core requirement, they were equivalent.

**Where they differed specifically:**
- **Claude** added a `StaffNotificationHub` with a `"staff"` SignalR group and `OnConnectedAsync` group-join, plus the `app.MapHub<>()` endpoint line — it thought about *who receives* the notification and wired the delivery target, not just the send. Its assumptions section flagged that the event name was a placeholder and that group membership might need role-gating.
- **ChatGPT** sent to `Clients.User(notification.UserId)` instead of a group, and included example `OrdersHub` and `PaidOrderNotification` models. It was slightly more minimal and made a different delivery assumption (per-user rather than a staff group), which didn't quite match "notify all staff."

**Honest verdict:** both were correct and shippable. Claude fit my "notify the staff group" intent more precisely because it reasoned about the audience; ChatGPT was a touch more generic on delivery but equally clean on the isolation requirement. Not "both were fine" — the specific difference was audience targeting (group vs single user).

---

## Final reusable template

Cleaned up so a stranger on my track could apply it to any coding task:

> You are a senior [LANGUAGE/STACK] developer. I'm building [ONE-LINE PROJECT CONTEXT]. I need [WHAT THE CODE MUST DO], and it must [THE KEY CONSTRAINT / WHY IT MATTERS].
>
> Here is the interface/style I use:
> ```
> [PASTE A SMALL EXAMPLE OF YOUR OWN CODE STYLE]
> ```
>
> Write [THE SPECIFIC THING]. Structure your answer as: (1) approach, (2) full code, (3) DI/registration, (4) assumptions. Show the code directly, do not create files.

The two techniques that mattered most: **context + motivation** (it chose the right architecture) and **few-shot example** (it matched my codebase instead of inventing its own).
