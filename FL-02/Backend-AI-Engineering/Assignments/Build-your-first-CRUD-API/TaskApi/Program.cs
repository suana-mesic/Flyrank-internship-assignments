using TaskApi.Models;
using TaskApi.Store;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ITaskStore, InMemoryTaskStore>();
var app = builder.Build();

app.MapGet("/", () => new { name = "Task API", version = "1.0", endpoints = new[] { "/tasks" } });
app.MapGet("/health", () => new { status = "ok" });

app.MapGet("/tasks", (ITaskStore store) => Results.Ok(store.GetAll()));

app.MapGet("/tasks/{id:int}", (int id, ITaskStore store) =>
{
    var task = store.GetById(id);
    return task is null
        ? Results.NotFound(new { error = $"Task {id} not found" })
        : Results.Ok(task);
});

app.MapPost("/tasks", (CreateTaskRequest req, ITaskStore store) =>
{
    if (string.IsNullOrWhiteSpace(req.Title))
        return Results.BadRequest(new { error = "Title is required" });

    var task = store.Add(req.Title.Trim());
    return Results.Created($"/tasks/{task.Id}", task);
});

// PUT /tasks/{id} -> updates title and/or done. Unknown id -> 404, empty/invalid body -> 400.
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

// DELETE /tasks/{id} -> 204 (no body) on success, 404 if the task doesn't exist.
app.MapDelete("/tasks/{id:int}", (int id, ITaskStore store) =>
{
    return store.Delete(id)
        ? Results.NoContent()
        : Results.NotFound(new { error = $"Task {id} not found" });
});

app.Run();
