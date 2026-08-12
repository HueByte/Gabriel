using Gabriel.Infrastructure.Memory;
using Xunit;

namespace Gabriel.Tests.Memory;

// The local-embedding conversion math (pooling, normalization, truncation,
// batch padding) is pure and tested here without any ONNX model on disk.
public class EmbeddingPipelineTests
{
    [Fact]
    public void MeanPool_averages_only_attended_tokens()
    {
        // 3 tokens x 2 dims; third token is padding and must not contribute.
        float[] hidden = [1f, 2f, 3f, 4f, 100f, 100f];
        long[] mask = [1, 1, 0];

        var pooled = EmbeddingPipeline.MeanPool(hidden, mask, dim: 2);

        Assert.Equal(2f, pooled[0]);
        Assert.Equal(3f, pooled[1]);
    }

    [Fact]
    public void MeanPool_all_padding_returns_zero_vector_not_NaN()
    {
        var pooled = EmbeddingPipeline.MeanPool([5f, 5f], [0], dim: 2);
        Assert.Equal([0f, 0f], pooled);
    }

    [Fact]
    public void Normalize_produces_unit_vector_and_leaves_zero_alone()
    {
        float[] v = [3f, 4f];
        EmbeddingPipeline.NormalizeInPlace(v);
        Assert.Equal(0.6f, v[0], precision: 5);
        Assert.Equal(0.8f, v[1], precision: 5);

        float[] zero = [0f, 0f];
        EmbeddingPipeline.NormalizeInPlace(zero);
        Assert.Equal([0f, 0f], zero);
    }

    [Fact]
    public void Truncate_keeps_trailing_special_token()
    {
        // [CLS]=101 ... [SEP]=102 — the SEP must survive truncation.
        var ids = new[] { 101, 5, 6, 7, 8, 9, 102 };
        var truncated = EmbeddingPipeline.Truncate(ids, maxTokens: 4);

        Assert.Equal(4, truncated.Count);
        Assert.Equal(101, truncated[0]);
        Assert.Equal(102, truncated[^1]);
    }

    [Fact]
    public void Truncate_is_identity_when_within_limit()
    {
        var ids = new[] { 101, 5, 102 };
        Assert.Same(ids, EmbeddingPipeline.Truncate(ids, maxTokens: 10));
    }

    [Fact]
    public void PadBatch_builds_rectangular_ids_and_mask()
    {
        var seqLen = EmbeddingPipeline.PadBatch(
            [new[] { 101, 7, 102 }, new[] { 101, 102 }],
            padTokenId: 0,
            out var inputIds,
            out var attentionMask);

        Assert.Equal(3, seqLen);
        Assert.Equal(new long[] { 101, 7, 102, 101, 102, 0 }, inputIds);
        Assert.Equal(new long[] { 1, 1, 1, 1, 1, 0 }, attentionMask);
    }
}
