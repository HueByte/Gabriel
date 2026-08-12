using Gabriel.Core.Configuration;
using Gabriel.Infrastructure.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Gabriel.Tests.Memory;

// End-to-end tests for the local ONNX embedding provider. They need the
// model files on disk (scripts/download-embedding-model.ps1) and no-op
// gracefully when absent, so CI without the ~90 MB download still passes —
// run the script locally to get real coverage.
public class LocalOnnxEmbeddingProviderTests
{
    private static LocalOnnxEmbeddingProvider? TryCreate()
    {
        var options = Options.Create(new EmbeddingOptions());
        return LocalOnnxEmbeddingProvider.TryResolveModelFiles(options.Value.Local, out _, out _)
            ? new LocalOnnxEmbeddingProvider(options, NullLogger<LocalOnnxEmbeddingProvider>.Instance)
            : null;
    }

    [Fact]
    public async Task Produces_normalized_vectors_of_configured_dimension()
    {
        using var provider = TryCreate();
        if (provider is null) return; // model not downloaded — soft skip

        var vector = await provider.EmbedAsync("the user prefers prose over bullet lists");

        Assert.Equal(384, vector.Length);
        var norm = MathF.Sqrt(vector.Sum(v => v * v));
        Assert.InRange(norm, 0.99f, 1.01f);
    }

    [Fact]
    public async Task Captures_actual_semantics_not_just_token_overlap()
    {
        using var provider = TryCreate();
        if (provider is null) return;

        // Paraphrase shares almost no tokens with the query; the distractor
        // shares none either. A real sentence encoder must rank the
        // paraphrase far closer — a bag-of-words model can't.
        var vectors = await provider.EmbedBatchAsync(
        [
            "How do I remove a saved memory?",
            "Deleting an entry that was previously stored",
            "The pixel avatar renders sixty-four animation frames",
        ]);

        var paraphrase = Dot(vectors[0], vectors[1]);
        var distractor = Dot(vectors[0], vectors[2]);
        Assert.True(paraphrase > distractor + 0.15f,
            $"expected paraphrase ({paraphrase:0.000}) well above distractor ({distractor:0.000})");
    }

    [Fact]
    public async Task Batch_and_single_agree_and_long_input_truncates()
    {
        using var provider = TryCreate();
        if (provider is null) return;

        var text = "semantic memory over qdrant";
        var single = await provider.EmbedAsync(text);
        var batch = await provider.EmbedBatchAsync([text, string.Join(' ', Enumerable.Repeat("word", 5000))]);

        Assert.Equal(single, batch[0]);
        Assert.Equal(384, batch[1].Length); // long input truncated, not crashed
    }

    private static float Dot(float[] a, float[] b)
    {
        var dot = 0f;
        for (var i = 0; i < a.Length; i++) dot += a[i] * b[i];
        return dot;
    }
}
