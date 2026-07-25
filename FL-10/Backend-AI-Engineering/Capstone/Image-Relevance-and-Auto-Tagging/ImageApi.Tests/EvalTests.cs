using ImageApi.Repositories;
using Xunit;

namespace ImageApi.Tests;

public class EvalTests
{
    // Same test Postgres (pgvector). Adjust if your .env differs.
    private const string ConnStr =
        "Host=localhost;Port=5435;Username=image_user;Password=image_pass;Database=imagedb";

    [Fact]
    public void TopOneMatch_PrecisionOnLabeledPosts_IsHigh()
    {
        var match = new MatchRepository(ConnStr);

        // Labeled eval set: each post's correct animal category.
        var cases = new (int postId, string expected)[]
        {
            (1, "fox"), (2, "wolf"), (3, "rabbit"), (4, "owl"), (5, "elephant")
        };

        int correct = 0;
        foreach (var (postId, expected) in cases)
        {
            var top = match.RankImagesForPost(postId, 1);
            Assert.NotEmpty(top);
            // Ground truth comes from the filename prefix: "fox_01.jpg" -> "fox".
            var predictedCategory = top[0].filename.Split('_')[0];
            if (predictedCategory == expected) correct++;
        }

        var precision = (double)correct / cases.Length;
        Assert.True(precision >= 0.8, $"Top-1 precision {precision:P0} is below 80%.");
    }
}