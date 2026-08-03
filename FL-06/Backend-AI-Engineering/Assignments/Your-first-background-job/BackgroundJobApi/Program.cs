using BackgroundJobApi.Models;
using BackgroundJobApi.Services;
using BackgroundJobApi.Store;
using System.Threading.Channels;

try { DotNetEnv.Env.TraversePath().Load(); } catch { }

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IJobStore, InMemoryJobStore>();
builder.Services.AddSingleton(Channel.CreateUnbounded<Guid>());
builder.Services.AddSingleton<IAlertService, ConsoleAlertService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<AiService>();
builder.Services.AddHostedService<JobWorker>();

var app = builder.Build();

app.MapGet("/", () => "Background Job API is running");

app.MapPost("/jobs", (JobRequest req, IJobStore store, Channel<Guid> channel) =>
{
    if (string.IsNullOrWhiteSpace(req.Text))
        return Results.BadRequest(new { error = "Text is required" });

    var job = new BackgroundJob
    {
        Id = Guid.NewGuid(),
        InputText = req.Text.Trim(),
        IdempotencyKey = req.IdempotencyKey
    };

    // Atomic: if the idempotency key is already taken, we get the existing job back.
    var (stored, isNew) = store.AddOrGet(job);

    if (!isNew)
        return Results.Ok(new { jobId = stored.Id, status = stored.Status.ToString() });

    channel.Writer.TryWrite(stored.Id);

    return Results.Accepted($"/jobs/{stored.Id}", new { jobId = stored.Id, status = "Queued" });
});

app.MapGet("/jobs/{id:guid}", (Guid id, IJobStore store) =>
{
    var job = store.GetById(id);
    if (job is null) return Results.NotFound();

    return Results.Ok(new
    {
        job.Id,
        Status = job.Status.ToString(),
        job.InputText,
        job.Result,
        job.Error,
        job.CreatedAtUtc,
        job.CompletedAtUtc
    });
});


app.Run();

public sealed record JobRequest(string Text, string? IdempotencyKey = null);
