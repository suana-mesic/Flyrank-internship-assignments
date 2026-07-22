using ImageApi.Database;
using ImageApi.Repositories;
using ImageApi.Services;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

var connStr = builder.Configuration.GetConnectionString("Images")
    ?? throw new InvalidOperationException("Missing ConnectionStrings__Images");

DatabaseInitializer.Initialize(connStr);

// Register repositories and the ingest service in the DI container.
builder.Services.AddSingleton(new ImageRepository(connStr));
builder.Services.AddSingleton(new PostRepository(connStr));
builder.Services.AddSingleton<IngestService>();

var app = builder.Build();

app.MapGet("/", () => "Image Relevance API is running");

// Loads every .jpg from the corpus folder into the images table.
app.MapPost("/ingest/images", (IngestService ingest, IConfiguration config) =>
{
    var corpus = config["Images:CorpusPath"]
        ?? throw new InvalidOperationException("Missing Images__CorpusPath");
    var count = ingest.IngestImages(corpus);
    return Results.Ok(new { inserted = count });
});

// Seeds the demo blog posts.
app.MapPost("/ingest/posts", (IngestService ingest) =>
{
    var count = ingest.SeedPosts();
    return Results.Ok(new { inserted = count });
});

app.Run();