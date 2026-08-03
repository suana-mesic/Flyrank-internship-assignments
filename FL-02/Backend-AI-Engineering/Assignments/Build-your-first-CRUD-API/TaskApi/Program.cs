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

// POST /tasks -> 201 + the created task. Missing/empty title -> 400.
app.MapPost("/tasks", (CreateTaskRequest req, ITaskStore store) =>
{
    if (string.IsNullOrWhiteSpace(req.Title))
        return Results.BadRequest(new { error = "Title is required" });

    var task = store.Add(req.Title.Trim());
    return Results.Created($"/tasks/{task.Id}", task);
});

app.Run();
