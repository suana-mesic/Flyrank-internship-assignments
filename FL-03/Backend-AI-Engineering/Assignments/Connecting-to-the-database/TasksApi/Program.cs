using Microsoft.Data.Sqlite;
using TasksApi.Database;

var builder = WebApplication.CreateBuilder(args);

var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "tasks.db");
var connectionString = $"Data Source={dbPath}";
DbInitializer.Initialize(connectionString);

var app = builder.Build();

// GET /tasks — svi taskovi
app.MapGet("/tasks", () =>
{
    using var conn = new SqliteConnection(connectionString);
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT id, title, done FROM tasks;";

    var tasks = new List<object>();
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        tasks.Add(new
        {
            id = reader.GetInt32(0),
            title = reader.GetString(1),
            done = reader.GetInt32(2) == 1
        });
    }

    return Results.Ok(tasks);
});

// GET /tasks/{id} — jedan task
app.MapGet("/tasks/{id:int}", (int id) =>
{
    using var conn = new SqliteConnection(connectionString);
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT id, title, done FROM tasks WHERE id = $id;";
    cmd.Parameters.AddWithValue("$id", id);

    using var reader = cmd.ExecuteReader();
    if (!reader.Read())
        return Results.NotFound(new { error = "Task not found" });

    return Results.Ok(new
    {
        id = reader.GetInt32(0),
        title = reader.GetString(1),
        done = reader.GetInt32(2) == 1
    });
});

// POST /tasks — novi task
app.MapPost("/tasks", (TaskRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Title))
        return Results.BadRequest(new { error = "Title is required" });

    using var conn = new SqliteConnection(connectionString);
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = "INSERT INTO tasks (title, done) VALUES ($title, 0) RETURNING id, title, done;";
    cmd.Parameters.AddWithValue("$title", req.Title.Trim());

    using var reader = cmd.ExecuteReader();
    reader.Read();

    return Results.Created($"/tasks/{reader.GetInt32(0)}", new
    {
        id = reader.GetInt32(0),
        title = reader.GetString(1),
        done = reader.GetInt32(2) == 1
    });
});

// PUT /tasks/{id} — ažuriraj task
app.MapPut("/tasks/{id:int}", (int id, TaskRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Title))
        return Results.BadRequest(new { error = "Title is required" });

    using var conn = new SqliteConnection(connectionString);
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = "UPDATE tasks SET title = $title, done = $done WHERE id = $id;";
    cmd.Parameters.AddWithValue("$title", req.Title.Trim());
    cmd.Parameters.AddWithValue("$done", req.Done ? 1 : 0);
    cmd.Parameters.AddWithValue("$id", id);

    return cmd.ExecuteNonQuery() > 0
        ? Results.NoContent()
        : Results.NotFound(new { error = "Task not found" });
});

// DELETE /tasks/{id} — obriši task
app.MapDelete("/tasks/{id:int}", (int id) =>
{
    using var conn = new SqliteConnection(connectionString);
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = "DELETE FROM tasks WHERE id = $id;";
    cmd.Parameters.AddWithValue("$id", id);

    return cmd.ExecuteNonQuery() > 0
        ? Results.NoContent()
        : Results.NotFound(new { error = "Task not found" });
});

app.Run();

public sealed record TaskRequest(string Title, bool Done = false);