# PDF Report Generator
**Assignment — Backend AI Engineering | Week 7**
**Intern:** Suana Mešić

The classic SaaS background job: query the data, render it into a PDF, and generate it out of band — so a request never blocks on report rendering and a big file never gets passed around in memory. Built on top of the Usage Metering & Billing service, so the data being reported is real (tenants, usage events, plans).

The request doesn't generate the PDF. It queues a job and returns immediately. A background worker picks the job up, renders the PDF, writes it to disk, and records where it is. The caller polls for status and downloads by link.

---

## Run it

The billing Postgres container already provides the data, so reuse it rather than starting a second one:

```bash
docker start billing-db
dotnet run --project BillingApi
```

The `reports` table is created at startup (`Database/init.sql`). Generated PDFs are written to the folder in `Reports__Path` (`.env`).

---

## Endpoints

| Method | Route | |
|---|---|---|
| POST | `/reports?period=YYYY-MM` | queue a report job; returns `202` + job id immediately |
| GET | `/reports/{id}` | job status, plus a download link once it's `done` |
| GET | `/reports/{id}/download` | streams the stored PDF from disk |

Two dev helpers used while building: `GET /reports/preview` (the report data as JSON) and `GET /reports/pdf` (render inline). The real flow is the three above.

---

## How it works

**Query — SQL aggregation.** `ReportRepository.GetReportData` rolls one tenant's usage for a month out of `usage_events`, joined to the tenant's plan for the limits. Cost comes from the existing `PricingService`.

**Render — QuestPDF.** `ReportService` turns that data into a one-page PDF: header (tenant, plan, period), a small metrics table (API calls and tokens, used vs limit), and the total cost. The QuestPDF license is declared once in a static constructor, so it's set whether the code runs from the app or from a test.

**Job — the A7 pattern, with the table as the queue.** `POST /reports` inserts a `pending` row and returns `202` right away. A `BackgroundService` worker polls the table and claims the oldest pending job atomically with `UPDATE ... WHERE id = (SELECT ... FOR UPDATE SKIP LOCKED)` — so two workers could run without ever grabbing the same job. It renders, saves, and moves the row `pending → processing → done` (or `failed` with the error).

**Artifact — store and link, don't pass 20 MB around.** The worker writes the PDF to disk and stores only the path in the row. The download endpoint streams the file straight from disk (`Results.File(path, ...)`), so the bytes are never held in memory or returned inline from the generating request.

---

## Tests

```bash
dotnet test
```

Three report tests (plus the existing billing suite):

| Test | What it checks |
|---|---|
| `GeneratePdf_ProducesValidPdfBytes` | rendering yields real PDF bytes (starts with the `%PDF` signature) — pure, no DB |
| `GetReportData_AggregatesTenantUsage` | the SQL aggregation returns the tenant's row with plan and usage |
| `JobLifecycle_PendingBecomesDone` | a queued job starts `pending` and transitions to `done` with a stored path |

---

## What's simplified, and the stretch

- **The report is intentionally small but meaningful** — one tenant, one month, usage vs limits and total cost. The pipeline (queue → render → store → link) is the point, not the layout.
- **On-demand now; scheduled is the stretch.** The same worker would run report jobs created by a schedule (e.g. a monthly trigger inserting a `pending` row per tenant) with no change to the generation path — only the trigger differs.
- **Per-job failure is marked, not retried.** A failed render marks the row `failed` with the reason and the worker moves on; a retry count would be a small addition.

---

## Files

```
PDF-report-generator/
└─ BillingApi/
   ├─ Models/ReportData.cs           the aggregated report row
   ├─ Repositories/ReportRepository.cs  SQL aggregation + job queue (claim/done/failed)
   ├─ Services/
   │  ├─ ReportService.cs            QuestPDF rendering
   │  └─ ReportWorker.cs             BackgroundService: poll → render → store
   └─ Program.cs                     /reports endpoints
   BillingApi.Tests/ReportTests.cs   3 report tests
```
