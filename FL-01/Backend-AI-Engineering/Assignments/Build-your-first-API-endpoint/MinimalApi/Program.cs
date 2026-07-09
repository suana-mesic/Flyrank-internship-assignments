var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// GET / -> welcome message
app.MapGet("/", () => new { message = "Hello from Suana's API" });

// GET /time -> current server time
app.MapGet("/time", () => new { utc = DateTime.UtcNow, local = DateTime.Now });

// GET /about -> a little about me
app.MapGet("/about", () => new
{
    name = "Suana Mešić",
    role = "Junior Backend Developer",
    stack = new[] { "C#", "ASP.NET Core", "TypeScript", "Angular", "Node.js" }
});

app.Run();