using TaskApi.Store;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ITaskStore, InMemoryTaskStore>();
var app = builder.Build();

app.MapGet("/", () => new { name = "Task API", version = "1.0", endpoints = new[] { "/tasks" } });
app.MapGet("/health", () => new { status = "ok" });

// GET /tasks -> the whole list.
app.MapGet("/tasks", (ITaskStore store) => Results.Ok(store.GetAll()));

// GET /tasks/{id} -> one task, or 404 with a JSON error if it doesn't exist.
app.MapGet("/tasks/{id:int}", (int id, ITaskStore store) =>
{
    var task = store.GetById(id);
    return task is null
        ? Results.NotFound(new { error = $"Task {id} not found" })
        : Results.Ok(task);
});

app.Run();
