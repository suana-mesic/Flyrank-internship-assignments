# FL-02: Prompting Fundamentals on Real Tasks — Prompt Iteration Log

**Track:** General AI Fluency | **Week:** 2 | **Phase:** Foundations
**Intern:** Suana Mešić — Junior Backend Developer

---

## The task

I took one real FL-01 task — writing code for BookVerse — and started from the naive prompt I'd actually type, then iterated through five versions, each adding one named technique. The iteration was done on **Claude (Sonnet)**; the final prompt was then run on both **Claude (Sonnet)** and **ChatGPT** for the cross-model comparison.

**Coding task:** a C# background service that notifies staff when a customer pays — something I built in BookVerse, so I can judge the output.

---

## Baseline — naive prompt

**Prompt:**
> Write a background service in C# that sends notifications. Show the code directly in your response, do not create files.

**Output:**
```csharp
// NotificationBackgroundService.cs
public class NotificationBackgroundService : BackgroundService
{
    private readonly INotificationSender _sender;
    private readonly ILogger<NotificationBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

    public NotificationBackgroundService(
        INotificationSender sender,
        ILogger<NotificationBackgroundService> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _sender.SendAsync(
                    "Status Update",
                    $"Service check at {DateTime.Now}",
                    stoppingToken);

                _logger.LogInformation("Notification sent at {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
```
It also generated a Windows-service `Program.cs` and an `EmailNotificationSender` over SMTP.

**What changed and why:** with no context, it invented a scenario — a timer firing every 15 minutes to email a generic "Status Update." Nothing to do with payments. A vague prompt gets a vague, invented answer.

---

## V1 — ROLE ASSIGNMENT

**Prompt:**
> **You are a senior .NET backend developer.** Write a background service in C# that sends notifications. Show the code directly in your response, do not create files.

**Output (core):**
```csharp
// INotificationQueue.cs
public interface INotificationQueue
{
    ValueTask EnqueueAsync(Notification notification, CancellationToken ct = default);
    IAsyncEnumerable<Notification> DequeueAllAsync(CancellationToken ct);
}

public sealed class InMemoryNotificationQueue : INotificationQueue
{
    private readonly Channel<Notification> _channel =
        Channel.CreateUnbounded<Notification>(new UnboundedChannelOptions { SingleReader = true });

    public ValueTask EnqueueAsync(Notification notification, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(notification, ct);

    public IAsyncEnumerable<Notification> DequeueAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);
}

// NotificationBackgroundService.cs (retry with backoff)
private async Task ProcessWithRetryAsync(Notification notification, CancellationToken ct)
{
    try
    {
        await _sender.SendAsync(notification, ct);
        _logger.LogInformation("Sent notification to {Recipient}", notification.Recipient);
    }
    catch (Exception ex) when (notification.Attempt < MaxAttempts)
    {
        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, notification.Attempt)), ct);
        await _queue.EnqueueAsync(notification with { Attempt = notification.Attempt + 1 }, ct);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Giving up after {Attempts} attempts", notification.Attempt);
    }
}
```

**What changed and why:** the code became production-shaped — a `Channel<T>` queue, exponential-backoff retry, graceful shutdown. But it still invented the scenario; the role alone made it *write like a senior dev*, not *solve my problem*.

---

## V2 — CONTEXT & MOTIVATION

**Prompt:**
> You are a senior .NET backend developer. **I'm building an ASP.NET Core e-commerce backend. When a customer pays for an order, staff (employee, manager, admin) need to be notified. The notification must not run inside the payment transaction, because if it fails it must not roll back the payment.** Write a background service in C# that handles this. Show the code directly in your response, do not create files.

**Output (core):**
```csharp
// OutboxMessage.cs
public sealed class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = default!;      // e.g. "OrderPaid"
    public string PayloadJson { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
}

// PaymentService.cs — outbox row written in the SAME transaction as the payment
_db.OutboxMessages.Add(new OutboxMessage
{
    Type = "OrderPaid",
    PayloadJson = JsonSerializer.Serialize(new OrderPaidPayload(
        order.Id, order.CustomerName, order.Total, order.PaidAtUtc.Value))
});
await _db.SaveChangesAsync(ct);
await transaction.CommitAsync(ct);

// StaffNotificationBackgroundService.cs — polls the outbox on its own loop,
// fresh DI scope per iteration, per-message try/catch, retry via Attempts counter
```

**What changed and why:** the biggest jump. Given the *why* (a failed notification must not roll back a payment), it recognised the problem and chose the **Transactional Outbox pattern** on its own — writing the notification to an outbox row atomically with the payment, then draining it in a separate loop. The motivation, not the role, drove the architecture.

---

## V3 — OUTPUT STRUCTURE

**Prompt:**
> You are a senior .NET backend developer. I'm building an ASP.NET Core e-commerce backend. When a customer pays for an order, staff need to be notified, but the notification must not run inside the payment transaction (a failure must not roll back the payment). Write a background service in C# that handles this. **Structure your answer as: (1) a short explanation of the approach, (2) the full code, (3) the DI registration line, (4) any assumptions you made.** Show the code directly in your response, do not create files.

**Output:** the same outbox implementation as V2, now organised into the four requested sections. The most useful addition was the explicit assumptions section, which surfaced what it had guessed:

> **4. Assumptions** — `AppDbContext` exposes `DbSet<OutboxMessage>` and `DbSet<Order>`. Staff roles are Employee, Manager, Admin, resolved via `IStaffDirectory`. `IStaffNotifier` is left abstract (email, SignalR, or push). `BackgroundService` is a singleton, so it creates a new DI scope per poll cycle to resolve the scoped `AppDbContext`. A 5-second poll interval is a starting point; swap for a broker if lower latency is needed.

**What changed and why:** the content was the same, but demanding a structure made it *reviewable*. The "assumptions" section exposed the guesses I would otherwise have had to hunt for in the code — the most valuable part of forcing an output shape.

---

## V4 — STEP DECOMPOSITION

**Prompt:**
> You are a senior .NET backend developer. I'm building an ASP.NET Core e-commerce backend. When a customer pays for an order, staff need to be notified, but the notification must not run inside the payment transaction (a failure must not roll back the payment). **Before writing code, reason through it step by step: first decide how the payment flow hands off the notification, then how the background service consumes it, then how a failure is isolated.** Then write the solution as: (1) approach, (2) full code, (3) DI registration, (4) assumptions. Show the code directly in your response, do not create files.

**Output (the new part — reasoning written before the code):**
> **Step 1 — how does the payment flow hand off the notification?** The payment write and the "staff needs to know" fact must be atomic, but delivery must not be. So the hand-off is persisted in the same DB transaction as the payment: an outbox row. That insert is just another row in the same `SaveChanges`, no network call, so it can't fail independently. If the transaction rolls back, the outbox row rolls back with it.
>
> **Step 2 — how does the background service consume it?** A `BackgroundService` polls the outbox table on its own loop, decoupled from any HTTP request. Because it reads durable storage rather than an in-memory queue, it survives restarts.
>
> **Step 3 — how is failure isolated?** Two boundaries: the service resolves `DbContext` from a fresh DI scope each iteration (it's a singleton), and each message is processed in its own try/catch so one bad send only increments that row's `Attempts` — it never throws out of the loop or reaches `PaymentService`.

The code body was the same outbox solution as V3.

**What changed and why:** the code barely changed, but the reasoning it wrote first made the *why* explicit — why the outbox insert can't fail independently, and why a singleton needs a fresh DI scope. This helped my understanding more than the code did.

---

## V5 — FEW-SHOT EXAMPLE

**Prompt:**
> You are a senior .NET backend developer. I'm building an ASP.NET Core e-commerce backend using the Transactional Outbox pattern. When a customer pays, the order is written to an outbox queue inside the payment transaction; a background service drains the queue and sends notifications over SignalR. **Here is the interface style I use:**
> ```csharp
> public interface IPaidOrderNotificationQueue
> {
>     IAsyncEnumerable<PaidOrderNotification> DequeueAllAsync(CancellationToken ct);
> }
> ```
> Write the background service that consumes this queue. A failure to send one notification must be logged and must not stop the loop or affect other notifications. Structure: (1) approach, (2) full code, (3) DI registration, (4) assumptions. Show the code directly in your response, do not create files.

**Output (Claude / Sonnet):**
```csharp
public sealed class PaidOrderNotificationBackgroundService : BackgroundService
{
    private readonly IPaidOrderNotificationQueue _queue;
    private readonly IHubContext<StaffNotificationHub> _hubContext;
    private readonly ILogger<PaidOrderNotificationBackgroundService> _logger;

    public PaidOrderNotificationBackgroundService(
        IPaidOrderNotificationQueue queue,
        IHubContext<StaffNotificationHub> hubContext,
        ILogger<PaidOrderNotificationBackgroundService> logger)
    {
        _queue = queue;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var notification in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                await _hubContext.Clients
                    .Group(StaffNotificationHub.StaffGroup)
                    .SendAsync("OrderPaid", new
                    {
                        notification.OrderId,
                        notification.CustomerName,
                        notification.Amount,
                        notification.PaidAtUtc
                    }, stoppingToken);
            }
            catch (Exception ex)
            {
                // Isolated per-notification: logged, loop continues.
                _logger.LogError(ex,
                    "Failed to send SignalR notification for order {OrderId}",
                    notification.OrderId);
            }
        }
    }
}

// DI + endpoint
builder.Services.AddSignalR();
builder.Services.AddHostedService<PaidOrderNotificationBackgroundService>();
app.MapHub<StaffNotificationHub>("/hubs/staff-notifications");
```

**What changed and why:** tightest and most usable output. The few-shot interface pinned it to my style — it wrote exactly the consumer I asked for, matched my `IAsyncEnumerable` / `await foreach` shape, delivered over SignalR, and placed the try/catch correctly *inside* the loop (it even explained that wrapping the whole `await foreach` would not isolate a single failed send). Giving it my interface stopped it inventing its own structure.

---

## Cross-model comparison (final prompt V5)

The same V5 few-shot prompt was run on **Claude (Sonnet)** and **ChatGPT**. Both produced a correct `await foreach` consumer with a per-notification try/catch and correct failure isolation. The difference was in *who receives the message*.

**Claude (Sonnet):** broadcast to a SignalR **group**, and wired the hub with a `MapHub` endpoint — reasoning explicitly about the "notify all staff" audience.
```csharp
await _hubContext.Clients
    .Group(StaffNotificationHub.StaffGroup)   // "staff"
    .SendAsync("OrderPaid", notification, stoppingToken);
// app.MapHub<StaffNotificationHub>("/hubs/staff-notifications");
```

**ChatGPT:** sent to a single **user** — cleaner, but a per-user assumption that doesn't match "notify all staff."
```csharp
await _hubContext
    .Clients
    .User(notification.UserId)                // one user, not the staff audience
    .SendAsync("OrderPaid", notification, stoppingToken);
// hub: public class OrdersHub : Hub { }
```

**Verdict:** both are shippable and structurally correct. Claude fit my actual intent ("notify staff") more precisely because it reasoned about the audience and set up group membership; ChatGPT's `Clients.User(userId)` would have quietly notified only one person. On this task, the audience reasoning was the deciding difference — not code quality.

---

## Final reusable template

> You are a senior [STACK] developer. I'm building [PROJECT CONTEXT]. I need [WHAT IT MUST DO], and it must [KEY CONSTRAINT / WHY].
> Here is the style I use: `[SMALL CODE EXAMPLE]`
> Write [THE THING]. Structure: (1) approach, (2) code, (3) registration, (4) assumptions.

The two techniques that mattered most: **context + motivation** (picked the right architecture — the Outbox pattern) and **few-shot example** (matched my codebase instead of inventing one). Role assignment and output structure improved shape and reviewability; step decomposition improved my understanding more than the code.
