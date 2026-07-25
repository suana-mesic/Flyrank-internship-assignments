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
builder.Services.AddSingleton(new CostRepository(connStr));
builder.Services.AddSingleton(new VectorRepository(connStr));
builder.Services.AddSingleton(new MatchRepository(connStr));
builder.Services.AddHttpClient<VisionService>(c => c.Timeout = TimeSpan.FromMinutes(5));
builder.Services.AddHttpClient<EmbeddingService>();
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

// Classifies a single image and records the cost of the call.
app.MapPost("/classify/{id:int}", async (int id,
    ImageRepository images, TagRepository tags, CostRepository costs,
    VisionService vision, IConfiguration config) =>
{
    var img = images.GetById(id);
    if (img is null) return Results.NotFound(new { error = "Image not found" });

    var threshold = double.TryParse(config["AI:ConfidenceThreshold"],
        System.Globalization.NumberStyles.Float,
        System.Globalization.CultureInfo.InvariantCulture, out var t) ? t : 0.6;
    var rate = decimal.TryParse(config["AI:CostPer1kTokens"],
        System.Globalization.NumberStyles.Float,
        System.Globalization.CultureInfo.InvariantCulture, out var r) ? r : 0m;
    var model = config["AI:VisionModel"] ?? "llava:13b";

    try
    {
        var result = await vision.ClassifyAsync(img.Value.filename);
        var flagged = result.Tags.Confidence < threshold;
        tags.Save(id, result.Tags, flagged, model);

        var cost = (result.PromptTokens + result.OutputTokens) / 1000m * rate;
        costs.Record("vision", model, result.PromptTokens, result.OutputTokens, cost);

        images.UpdateStatus(id, "classified");
        return Results.Ok(new { image = img.Value.filename, tags = result.Tags, flagged });
    }
    catch (Exception ex)
    {
        images.UpdateStatus(id, "failed");
        return Results.Json(new { error = "Classification failed", detail = ex.Message }, statusCode: 500);
    }
});

// Classifies all pending images (optionally only the first ?limit), with
// per-image retries, recording cost for each call. Failures are marked and skipped.
app.MapPost("/classify/batch", async (int? limit,
    ImageRepository images, TagRepository tags, CostRepository costs,
    VisionService vision, IConfiguration config) =>
{
    var pending = images.GetByStatus("pending");
    if (limit is > 0) pending = pending.Take(limit.Value).ToList();

    var threshold = double.TryParse(config["AI:ConfidenceThreshold"],
        System.Globalization.NumberStyles.Float,
        System.Globalization.CultureInfo.InvariantCulture, out var t) ? t : 0.6;
    var rate = decimal.TryParse(config["AI:CostPer1kTokens"],
        System.Globalization.NumberStyles.Float,
        System.Globalization.CultureInfo.InvariantCulture, out var r) ? r : 0m;
    var model = config["AI:VisionModel"] ?? "llava:13b";

    int ok = 0, failed = 0;
    foreach (var img in pending)
    {
        const int maxAttempts = 3;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                var result = await vision.ClassifyAsync(img.filename);
                var flagged = result.Tags.Confidence < threshold;
                tags.Save(img.id, result.Tags, flagged, model);

                var cost = (result.PromptTokens + result.OutputTokens) / 1000m * rate;
                costs.Record("vision", model, result.PromptTokens, result.OutputTokens, cost);

                images.UpdateStatus(img.id, "classified");
                Console.WriteLine($"[classify] OK {img.filename} -> {result.Tags.Subject}");
                ok++;
                break;
            }
            catch (Exception) when (attempt < maxAttempts)
            {
                // transient failure: wait and retry this image
                await Task.Delay(TimeSpan.FromSeconds(2 * attempt));
            }
            catch (Exception ex)
            {
                images.UpdateStatus(img.id, "failed");
                Console.WriteLine($"[classify] FAIL {img.filename}: {ex.Message}");
                failed++;
                break;
            }
        }
    }

    return Results.Ok(new { classified = ok, failed });
});


// Embeds every image caption into image_vectors (records cost per call).
app.MapPost("/embed/images", async (TagRepository tags, VectorRepository vectors,
    CostRepository costs, EmbeddingService embed, IConfiguration config) =>
{
    var rate = decimal.TryParse(config["AI:CostPer1kTokens"],
       System.Globalization.NumberStyles.Float,
       System.Globalization.CultureInfo.InvariantCulture, out var r) ? r : 0m;
    var model = config["AI:EmbedModel"] ?? "nomic-embed-text";

    var captions = tags.GetAllCaptions();
    int done = 0;

    foreach(var (imageId, caption) in captions)
    {
        var (vec, tokens) = await embed.EmbedAsync(caption);
        vectors.SaveImageVector(imageId, vec);
        costs.Record("embedding", model, tokens, 0, tokens / 1000m * rate);
        done++;
    }
    return Results.Ok(new { embedded = done });
});

// Embeds every post (title + body) into post_vectors (records cost per call).
app.MapPost("/embed/posts", async (PostRepository posts, VectorRepository vectors,
    CostRepository costs, EmbeddingService embed, IConfiguration config) =>
{
    var rate = decimal.TryParse(config["AI:CostPer1kTokens"],
        System.Globalization.NumberStyles.Float,
        System.Globalization.CultureInfo.InvariantCulture, out var r) ? r : 0m;
    var model = config["AI:EmbedModel"] ?? "nomic-embed-text";

    var allPosts = posts.GetAll();
    int done = 0;
    foreach (var (id, title, body) in allPosts)
    {
        var (vec, tokens) = await embed.EmbedAsync($"{title}. {body}");
        vectors.SavePostVector(id, vec);
        costs.Record("embedding", model, tokens, 0, tokens / 1000m * rate);
        done++;
    }
    return Results.Ok(new { embedded = done });
});

// Returns the top-ranked images for a post, by semantic similarity.
app.MapGet("/posts/{id:int}/matches", (int id, MatchRepository match, int? limit) =>
{
    var top = match.RankImagesForPost(id, limit ?? 5);
    return Results.Ok(new
    {
        postId = id,
        matches = top.Select(m => new
        {
            m.imageId,
            m.filename,
            m.subject,
            similarity = Math.Round(m.similarity, 4)
        })
    });
});

app.Run();