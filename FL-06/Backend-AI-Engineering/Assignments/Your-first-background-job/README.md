# BE-06: Your First Background Job

**Track:** Backend AI Engineering | **Week:** 6 | **Phase:** Build
**Intern:** Suana Mešić

---

## What it does

A text summarization API that accepts a request, responds immediately with `202 Accepted` and a job ID, then processes the work in the background using an AI model. The caller checks back with `GET /jobs/{id}` to see the result.

The AI call goes to Groq (Llama 3.1 8B) — free tier, 14,400 requests/day.

---

## Endpoints

| Method | Route | What it does |
|---|---|---|
| POST | `/jobs` | Accept text, return 202 + job ID immediately |
| GET | `/jobs/{id}` | Check job status and get the result |

### POST /jobs

```json
{
  "text": "Your text to summarize...",
  "idempotencyKey": "optional-unique-key"
}
```

Response: `202 Accepted`
```json
{
  "jobId": "aa88dfc5-...",
  "status": "Queued"
}
```

### GET /jobs/{id}

```json
{
  "id": "aa88dfc5-...",
  "status": "Completed",
  "inputText": "Your text...",
  "result": "AI-generated summary...",
  "error": null,
  "createdAtUtc": "2026-07-18T13:16:28Z",
  "completedAtUtc": "2026-07-18T13:16:29Z"
}
```

Status values: `Queued` → `Processing` → `Completed` or `Failed`.

---

## How it works

```
POST /jobs
  │
  ├─ validate input
  ├─ check idempotency key → if exists, return existing job
  ├─ create job (status: Queued)
  ├─ write job ID into a Channel (in-memory queue)
  └─ return 202 immediately
        │
        │  (background, separate thread)
        ▼
   JobWorker picks up the ID
  ├─ set status → Processing
  ├─ call Groq API (with up to 3 retries)
  ├─ on success → status: Completed, store result
  └─ on failure → status: Failed, store error
        │
GET /jobs/{id}
  └─ read job from store, return current status + result
```

The caller never waits for the AI call. The endpoint returns before the work starts.

---

## Key design decisions

### Background processing via Channel + BackgroundService

The POST endpoint writes a job ID into a `Channel<Guid>`. A `BackgroundService` (`JobWorker`) reads from the other end and does the slow work. This is the same pattern I used in my BookVerse project (outbox + `PaidOrderNotificationBackgroundService`) and in my capstone (`WebhookBackgroundService`). The difference here is that the worker writes a result back into the job, so the caller can retrieve it later.

### Idempotency

If the caller sends an `idempotencyKey`, the API checks whether a job with that key already exists. If it does, it returns the existing job instead of creating a duplicate. This protects against retries — if the caller's network drops after sending the request, they can safely resend with the same key and get the same job back, not a second one.

### Retry with backoff

The worker retries the AI call up to 3 times with progressive delays (1s, 2s, 3s). If all three fail, the job is marked as `Failed` with the error message. One failed job never stops the worker loop — the `try/catch` sits inside the `await foreach`, so the next job is processed normally.

### In-memory store

Jobs live in a `ConcurrentDictionary` — singleton, thread-safe. No database. This means jobs are lost on restart, which is acceptable for this assignment. A production version would use Postgres.

---

## Run it

```bash
cp .env.example .env       # then add your Groq API key
dotnet run --project BackgroundJobApi
```

Test:
```bash
# submit a job
curl -X POST http://localhost:5067/jobs \
  -H "Content-Type: application/json" \
  -d '{"text": "Artificial intelligence is transforming healthcare."}'

# check the result (replace the ID)
curl http://localhost:5067/jobs/{id}

# test idempotency — same key, same job
curl -X POST http://localhost:5067/jobs \
  -H "Content-Type: application/json" \
  -d '{"text": "Test", "idempotencyKey": "abc123"}'

curl -X POST http://localhost:5067/jobs \
  -H "Content-Type: application/json" \
  -d '{"text": "Test", "idempotencyKey": "abc123"}'
# → same jobId both times
```

---

## Files

```
Your-first-background-job/
├─ BackgroundJobApi/
│  ├─ Models/BackgroundJob.cs         job entity with status enum
│  ├─ Store/IJobStore.cs              interface
│  ├─ Store/InMemoryJobStore.cs       ConcurrentDictionary, singleton
│  ├─ Services/AiService.cs           Groq API call
│  ├─ Services/JobWorker.cs           BackgroundService, retry loop
│  └─ Program.cs                      endpoints + DI
├─ BackgroundJobApi.sln
├─ .env.example
└─ .gitignore
```
