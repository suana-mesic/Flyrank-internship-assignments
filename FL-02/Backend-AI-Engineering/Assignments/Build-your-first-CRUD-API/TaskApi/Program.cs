using TaskApi.Models;
using TaskApi.Store;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITaskStore, InMemoryTaskStore>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new() { Title = "Task API", Version = "1.0" });
});

var app = builder.Build();

// Swagger UI served at /docs (the OpenAPI JSON lives at /swagger/v1/swagger.json).
app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "Task API v1");
    o.RoutePrefix = "docs";
});

// --- Root & health ---
app.MapGet("/", () => new { name = "Task API", version = "1.0", endpoints = new[] { "/tasks" } });
app.MapGet("/health", () => new { status = "ok" });

// --- Read ---
app.MapGet("/tasks", (ITaskStore store, bool? done, string? search) =>
{
    IEnumerable<TaskItem> tasks = store.GetAll();

    if (done is not null)
        tasks = tasks.Where(t => t.Done == done);

    if (!string.IsNullOrWhiteSpace(search))
        tasks = tasks.Where(t => t.Title.Contains(search, StringComparison.OrdinalIgnoreCase));

    return Results.Ok(tasks.ToList());
});

app.MapGet("/tasks/{id:int}", (int id, ITaskStore store) =>
{
    var task = store.GetById(id);
    return task is null
        ? Results.NotFound(new { error = $"Task {id} not found" })
        : Results.Ok(task);
});

// --- Create ---
app.MapPost("/tasks", (CreateTaskRequest req, ITaskStore store) =>
{
    if (string.IsNullOrWhiteSpace(req.Title))
        return Results.BadRequest(new { error = "Title is required" });

    var task = store.Add(req.Title.Trim());
    return Results.Created($"/tasks/{task.Id}", task);
});

// --- Update ---
app.MapPut("/tasks/{id:int}", (int id, UpdateTaskRequest req, ITaskStore store) =>
{
    if (req is null || (req.Title is null && req.Done is null))
        return Results.BadRequest(new { error = "Provide a title and/or done value" });

    if (req.Title is not null && string.IsNullOrWhiteSpace(req.Title))
        return Results.BadRequest(new { error = "Title cannot be empty" });

    var updated = store.Update(id, req.Title?.Trim(), req.Done);
    return updated is null
        ? Results.NotFound(new { error = $"Task {id} not found" })
        : Results.Ok(updated);
});

// --- Delete ---
app.MapDelete("/tasks/{id:int}", (int id, ITaskStore store) =>
{
    return store.Delete(id)
        ? Results.NoContent()
        : Results.NotFound(new { error = $"Task {id} not found" });
});

// --- Optional extras ---
app.MapGet("/stats", (ITaskStore store) =>
{
    var all = store.GetAll();
    return Results.Ok(new { total = all.Count, done = all.Count(t => t.Done), open = all.Count(t => !t.Done) });
});

app.MapPost("/reset", (ITaskStore store) =>
{
    store.Reset();
    return Results.Ok(new { message = "Tasks reset to the 3 seed items" });
});

app.Run();
