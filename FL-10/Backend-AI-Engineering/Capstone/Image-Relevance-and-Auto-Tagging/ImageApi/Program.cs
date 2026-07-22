using ImageApi.Database;

// Load .env (POSTGRES_* and ConnectionStrings__Images) into configuration.
DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

var connStr = builder.Configuration.GetConnectionString("Images")
    ?? throw new InvalidOperationException("Missing ConnectionStrings__Images");

// Create the schema (pgvector + all tables) at startup.
DatabaseInitializer.Initialize(connStr);

var app = builder.Build();

app.MapGet("/", () => "Image Relevance API is running");

app.Run();