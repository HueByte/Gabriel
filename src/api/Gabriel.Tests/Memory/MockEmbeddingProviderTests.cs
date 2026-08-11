using Gabriel.Core.Configuration;
using Gabriel.Infrastructure.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Gabriel.Tests.Memory;

public class MockEmbeddingProviderTests
{
    private static MockEmbeddingProvider Create(int dimensions = 256)
        => new(Options.Create(new EmbeddingOptions { MockDimensions = dimensions }));

    [Fact]
    public async Task Embedding_is_deterministic()
    {
        var provider = Create();
        var a = await provider.EmbedAsync("the user prefers prose over bullet lists");
        var b = await provider.EmbedAsync("the user prefers prose over bullet lists");
        Assert.Equal(a, b);
    }

    [Fact]
    public async Task Embedding_is_unit_normalized()
    {
        var provider = Create();
        var v = await provider.EmbedAsync("semantic memory with qdrant vectors");
        var norm = MathF.Sqrt(v.Sum(x => x * x));
        Assert.InRange(norm, 0.999f, 1.001f);
    }

    [Fact]
    public async Task Related_text_scores_higher_than_unrelated()
    {
        var provider = Create();
        var query = await provider.EmbedAsync("docker compose services and containers");
        var related = await provider.EmbedAsync("the docker compose file defines the api and webapp services");
        var unrelated = await provider.EmbedAsync("gabriel mirrors the user's emoji and lowercase style");

        Assert.True(Cosine(query, related) > Cosine(query, unrelated));
    }

    [Fact]
    public async Task Batch_matches_single_embeddings()
    {
        var provider = Create();
        var batch = await provider.EmbedBatchAsync(["alpha beta", "gamma delta"]);
        Assert.Equal(await provider.EmbedAsync("alpha beta"), batch[0]);
        Assert.Equal(await provider.EmbedAsync("gamma delta"), batch[1]);
    }

    [Fact]
    public void Dimensions_respect_configured_floor()
    {
        Assert.Equal(16, Create(dimensions: 4).Dimensions);
        Assert.Equal(512, Create(dimensions: 512).Dimensions);
    }

    private static float Cosine(float[] a, float[] b)
    {
        var dot = 0f;
        for (var i = 0; i < a.Length; i++) dot += a[i] * b[i];
        return dot; // both unit-normalized
    }
}
