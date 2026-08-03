var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// GET / -> describes the API.
app.MapGet("/", () => new { name = "Task API", version = "1.0", endpoints = new[] { "/tasks" } });

// GET /health -> liveness probe used by real infrastructure to check the server is up.
app.MapGet("/health", () => new { status = "ok" });

app.Run();
