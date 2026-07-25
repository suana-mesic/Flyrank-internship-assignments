using ImageApi.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ImageApi.Tests;

public class GuardServiceTests
{
    private static GuardService MakeGuard(double threshold = 0.67)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MATCH:SimilarityThreshold"] =
                    threshold.ToString(System.Globalization.CultureInfo.InvariantCulture)
            })
            .Build();
        return new GuardService(config);
    }

    [Fact]
    public void WolfOnFoxPost_HighSimilarity_RejectedByTagMismatch()
    {
        // Rule B: even with high similarity, a wolf image on a fox post is refused.
        var result = MakeGuard().Evaluate("fox", "Wolf", 0.80);
        Assert.Equal(GuardDecision.RejectTagMismatch, result.Decision);
    }

    [Fact]
    public void CorrectFox_Accepted()
    {
        var result = MakeGuard().Evaluate("fox", "red fox", 0.80);
        Assert.Equal(GuardDecision.Accept, result.Decision);
    }

    [Fact]
    public void LowSimilarity_Rejected()
    {
        // Rule A: below threshold -> no confident match.
        var result = MakeGuard().Evaluate("fox", "red fox", 0.50);
        Assert.Equal(GuardDecision.RejectLowSimilarity, result.Decision);
    }
}