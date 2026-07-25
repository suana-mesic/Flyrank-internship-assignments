using ImageApi.Database;
using ImageApi.Repositories;
using ImageApi.Services;
using Microsoft.Extensions.FileProviders;

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
builder.Services.AddSingleton(new PairingRepository(connStr));
builder.Services.AddHttpClient<VisionService>(c => c.Timeout = TimeSpan.FromMinutes(5));
builder.Services.AddHttpClient<EmbeddingService>();
builder.Services.AddSingleton<IngestService>();
builder.Services.AddSingleton<GuardService>();    
builder.Services.AddSingleton<PairingService>();


var app = builder.Build();

// Serve the corpus images at /corpus/<filename> so the review page can show them.
var corpusPath = builder.Configuration["Images:CorpusPath"]!;
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(corpusPath),
    RequestPath = "/corpus"
});

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

// Suggests the best image for a post, or refuses with a reason if the guard rejects it.
app.MapGet("/posts/{id:int}/suggestion", (int id,
    PostRepository posts, MatchRepository match, GuardService guard) =>
{
    var post = posts.GetById(id);
    if (post is null) return Results.NotFound(new { error = "Post not found" });

    var ranked = match.RankImagesForPost(id, 1);
    if (ranked.Count == 0) return Results.Ok(new { status = "no candidates" });

    var best = ranked[0];
    var verdict = guard.Evaluate(post.Value.topic, best.subject, best.similarity);

    if (verdict.Decision == GuardDecision.Accept)
        return Results.Ok(new
        {
            status = "suggested",
            image = best.filename,
            best.subject,
            similarity = Math.Round(best.similarity, 4),
            reason = verdict.Reason
        });

    return Results.Ok(new
    {
        status = "no good match",
        reason = verdict.Reason,
        bestCandidate = new { best.filename, best.subject, similarity = Math.Round(best.similarity, 4) }
    });
});

// Evaluates a forced (post, image) pairing — used to prove the guard rejects a wrong image.
app.MapGet("/posts/{id:int}/guard/{imageId:int}", (int id, int imageId,
    PostRepository posts, TagRepository tags, MatchRepository match, GuardService guard) =>
{
    var post = posts.GetById(id);
    var subject = tags.GetSubject(imageId);
    var sim = match.SimilarityFor(id, imageId);
    if (post is null || subject is null || sim is null)
        return Results.NotFound(new { error = "Post or image not found / not embedded" });

    var verdict = guard.Evaluate(post.Value.topic, subject, sim.Value);
    return Results.Ok(new
    {
        post = post.Value.slug,
        imageId,
        subject,
        similarity = Math.Round(sim.Value, 4),
        decision = verdict.Decision.ToString(),
        reason = verdict.Reason
    });
});

// Suggest for one post.
app.MapPost("/posts/{id:int}/suggest", (int id, PairingService pairing) =>
{
    var result = pairing.SuggestForPost(id);
    return result is null ? Results.NotFound(new { error = "Post not found" }) : Results.Ok(result);
});

// Suggest for every post (populates the review table in one call).
app.MapPost("/suggest-all", (PostRepository posts, PairingService pairing) =>
{
    var results = posts.GetAll().Select(p => pairing.SuggestForPost(p.id)).ToList();
    return Results.Ok(new { suggested = results.Count });
});

// Approve / reject a pairing.
app.MapPost("/pairings/{id:int}/approve", (int id, PairingRepository pairings) =>
    pairings.SetStatus(id, "approved") ? Results.Ok(new { id, status = "approved" })
                                       : Results.NotFound());

app.MapPost("/pairings/{id:int}/reject", (int id, PairingRepository pairings) =>
    pairings.SetStatus(id, "rejected") ? Results.Ok(new { id, status = "rejected" })
                                       : Results.NotFound());

// The one-page review surface: a table of pairings with approve/reject buttons.
app.MapGet("/review", (PairingRepository pairings) =>
{
    var rows = pairings.GetAll();
    var sb = new System.Text.StringBuilder();
    sb.Append("""
        <!doctype html><html><head><meta charset="utf-8"><title>Image Review</title>
        <style>
          body{font-family:system-ui,sans-serif;margin:24px;color:#111}
          table{border-collapse:collapse;width:100%}
          th,td{border:1px solid #ddd;padding:8px;text-align:left;vertical-align:top}
          img{height:64px;border-radius:4px}
          .no-match{color:#b00}
          button{cursor:pointer;padding:4px 10px;margin-right:4px}
        </style></head><body>
        <h1>Image Review</h1>
        <table><tr><th>Post</th><th>Image</th><th>Subject</th><th>Similarity</th>
        <th>Status</th><th>Reason</th><th>Actions</th></tr>
        """);

    foreach (var r in rows)
    {
        var img = r.Filename is null
            ? "<span class='no-match'>— no match —</span>"
            : $"<img src='/corpus/{r.Filename}' alt='{r.Filename}'><br>{r.Filename}";
        var sim = r.Similarity is null ? "" : r.Similarity.Value.ToString("0.00");
        sb.Append($"""
            <tr>
              <td>{r.PostSlug}</td>
              <td>{img}</td>
              <td>{r.Subject ?? ""}</td>
              <td>{sim}</td>
              <td>{r.Status}</td>
              <td>{r.Reason ?? ""}</td>
              <td>
                <button onclick="act({r.Id},'approve')">Approve</button>
                <button onclick="act({r.Id},'reject')">Reject</button>
              </td>
            </tr>
            """);
    }

    sb.Append("""
        </table>
        <script>
          async function act(id, what) {
            await fetch(`/pairings/${id}/${what}`, { method: 'POST' });
            location.reload();
          }
        </script>
        </body></html>
        """);

    return Results.Content(sb.ToString(), "text/html");
});

app.Run();