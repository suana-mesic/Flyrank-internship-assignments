using System.Text.Json;
using ImageApi.Models;
using Xunit;

namespace ImageApi.Tests;

public class SchemaValidationTests
{
    private static readonly JsonSerializerOptions Opts = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public void ValidVisionJson_ParsesIntoAllFields()
    {
        var json = """
            {"subject":"red fox","category":"animal",
             "attributes":["orange fur","snow"],
             "caption":"A red fox walking in snow","confidence":0.92}
            """;

        var tags = JsonSerializer.Deserialize<ImageTags>(json, Opts)!;

        Assert.Equal("red fox", tags.Subject);
        Assert.Equal("animal", tags.Category);
        Assert.Equal(2, tags.Attributes.Length);
        Assert.False(string.IsNullOrWhiteSpace(tags.Caption));
        Assert.InRange(tags.Confidence, 0.0, 1.0);
    }

    [Fact]
    public void MalformedJson_IsRejected()
    {
        var bad = "{ this is not valid json";
        Assert.ThrowsAny<Exception>(() => JsonSerializer.Deserialize<ImageTags>(bad, Opts));
    }
}