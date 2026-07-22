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
builder.Services.AddSingleton(new TagRepository(connStr));
builder.Services.AddHttpClient<VisionService>(c => c.Timeout = TimeSpan.FromMinutes(5));
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

app.MapPost("/classify/{id:int}", async (int id, ImageRepository images, TagRepository tags, VisionService vision, IConfiguration config) =>
{
    var img = images.GetById(id);
    if (img is null) return Results.NotFound(new { error = "Image not found" });

    var threshold = double.TryParse(config["AI:ConfidenceThreshold"],
        System.Globalization.NumberStyles.Float,
        System.Globalization.CultureInfo.InvariantCulture, out var t) ? t : 0.6;
    var model = config["AI:VisionModel"] ?? "gemini-2.0-flash";
    try
    {
        var result = await vision.ClassifyAsync(img.Value.filename);
        var flagged = result.Confidence < threshold;  // "low confidence -> flag, d
        tags.Save(id, result, flagged, model);
        images.UpdateStatus(id, "classified");
        return Results.Ok(new { image = img.Value.filename, tags = result, flagged });
    }
    catch (Exception ex)
    {
        images.UpdateStatus(id, "failed");
        return Results.Json(new { error = "Classification failed", detail = ex.Message }, statusCode: 500);
    }
});

app.Run();