# BE-01: Build Your First API Endpoint

**Track:** Backend AI Engineering
**Week:** 1 | **Phase:** Setup
**Intern:** Suana Mešić — Junior Backend Developer

---

## Goal

The smallest possible backend: an ASP.NET Core Minimal API with a few JSON endpoints, callable from both the browser and curl, published to a public GitHub repository.

---

## Endpoints

| Method | Route | Response |
|--------|-------|----------|
| GET | `/` | `{ "message": "Hello from Suana's API" }` |
| GET | `/time` | Current server time (UTC and local) |
| GET | `/about` | Name, role, and tech stack |

---

## How to Run

```bash
dotnet run
```

Then open in the browser (replace the port with the one shown in the console):

- http://localhost:5031/
- http://localhost:5031/time
- http://localhost:5031/about

Or call from curl:

```bash
curl http://localhost:5031/
curl http://localhost:5031/time
curl http://localhost:5031/about
```

---

## The Code

The entire backend is a handful of lines in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => new { message = "Hello from Suana's API" });

app.MapGet("/time", () => new { utc = DateTime.UtcNow, local = DateTime.Now });

app.MapGet("/about", () => new
{
    name = "Suana Mešić",
    role = "Junior Backend Developer",
    stack = new[] { "C#", "ASP.NET Core", "TypeScript", "Angular", "Node.js" }
});

app.Run();
```

---

## What I Learned

In the lecture I watched the request → response loop from the outside. Here I stood on the server side of it: an endpoint receives a request and returns a JSON response. Minimal API makes this genuinely small — no controllers, no boilerplate — which made the request/response loop concrete instead of abstract.
