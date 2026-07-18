using BackgroundJobApi.Models;
using BackgroundJobApi.Services;
using BackgroundJobApi.Store;
using System.Threading.Channels;

try { DotNetEnv.Env.TraversePath().Load(); } catch { }

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IJobStore, InMemoryJobStore>();
builder.Services.AddSingleton(Channel.CreateUnbounded<Guid>());
builder.Services.AddHttpClient();
builder.Services.AddScoped<AiService>();
builder.Services.AddHostedService<JobWorker>();

var app = builder.Build();

app.MapGet("/", () => "Background Job API is running");

app.MapPost("/jobs", (JobRequest req, IJobStore store, Channel<Guid> channel) =>
{
    if (string.IsNullOrWhiteSpace(req.Text))
        return Results.BadRequest(new { error = "Text is required" });

    if (!string.IsNullOrWhiteSpace(req.IdempotencyKey))
    {
        var existing = store.GetByIdempotencyKey(req.IdempotencyKey);

        if (existing is not null)
            return Results.Ok(new { jobId = existing.Id, status = existing.Status.ToString() });
    }

    var job = new BackgroundJob
    {
        Id = Guid.NewGuid(),
        InputText = req.Text.Trim(),
        IdempotencyKey = req.IdempotencyKey
    };

    store.Add(job);
    channel.Writer.TryWrite(job.Id);

    return Results.Accepted($"/jobs/{job.Id}", new { jobid = job.Id, status = "Queued" });
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
