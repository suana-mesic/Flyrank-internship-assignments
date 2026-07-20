using AuthApi.Services;
using System.Text.Json;

try { DotNetEnv.Env.TraversePath().Load(); } catch { }

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddScoped<SupabaseAuthService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Auth API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste your access token here"
    });
    options.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<SupabaseAuthService>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "Auth API is running");

app.MapPost("/auth/signup", async (AuthRequest req, SupabaseAuthService auth) =>
{
    if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { error = "Email and password are required" });

    try
    {
        var user = await auth.SignUpAsync(req.Email, req.Password);
        return Results.Created("/auth/login", user);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/auth/login", async (AuthRequest req, SupabaseAuthService auth) =>
{
    if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { error = "Email and password are required" });

    try
    {
        var result = await auth.SignInAsync(req.Email, req.Password);
        return Results.Ok(result);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Json(new { error = "Invalid login credentials" }, statusCode: 401);
    }
});

// --- PUBLIC ---

app.MapGet("/public/info", () =>
    Results.Ok(new { message = "Welcome stranger! This info is public." }));

// --- PROTECTED ---
app.MapGet("/protected/profile", async (HttpContext http, SupabaseAuthService auth) =>
{
    var (ok, user) = await VerifyToken(http, auth);
    return ok
        ? Results.Ok(user)
        : Results.Json(new { error = "Invalid or expired token" }, statusCode: 401);
});

app.MapGet("/protected/dashboard", async (HttpContext http, SupabaseAuthService auth) =>
{
    var (ok, user) = await VerifyToken(http, auth);
    return ok
        ? Results.Ok(new { message = "Welcome to the dashboard", user })
        : Results.Json(new { error = "Invalid or expired token" }, statusCode: 401);
});

// --- LOGOUT ---

app.MapPost("/auth/logout", async (HttpContext http, SupabaseAuthService auth) =>
{
    var token = ExtractToken(http);
    if (token is null)
        return Results.Json(new { error = "Access token required" }, statusCode: 401);

    try
    {
        await auth.LogoutAsync(token);
        return Results.NoContent();
    }
    catch
    {
        return Results.Json(new { error = "Logout failed" }, statusCode: 401);
    }
});

// --- MIDDLEWARE ---
async Task<(bool ok, JsonElement user)> VerifyToken(HttpContext http, SupabaseAuthService auth)
{
    var token = ExtractToken(http);
    if (token is null) return (false, default);

    try
    {
        var user = await auth.GetUserAsync(token);
        return (true, user);
    }
    catch (UnauthorizedAccessException)
    {
        return (false, default);
    }
}
static string? ExtractToken(HttpContext http)
{
    var header = http.Request.Headers.Authorization.ToString();
    if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Bearer "))
        return null;
    return header["Bearer ".Length..];
}


app.Run();
public sealed record AuthRequest(string Email, string Password);