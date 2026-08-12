namespace Gabriel.Infrastructure.Memory;

// The text→vector conversion math shared by local embedding models, kept as
// pure static functions so the pipeline is unit-testable without an ONNX
// model on disk. This is the standard sentence-transformers recipe:
// attention-masked mean pooling over the token embeddings, then L2
// normalization so stored vectors compare by plain dot product / cosine.
public static class EmbeddingPipeline
{
    // Mean-pools one sequence's token embeddings using its attention mask.
    // `hidden` is the flattened [seqLen, dim] slice for a single input;
    // padded positions (mask 0) contribute nothing. Falls back to a zero
    // vector if the mask is all-zero (can't happen with [CLS]/[SEP] present,
    // but a math helper shouldn't divide by zero on principle).
    public static float[] MeanPool(ReadOnlySpan<float> hidden, ReadOnlySpan<long> attentionMask, int dim)
    {
        var pooled = new float[dim];
        var tokens = 0;
        for (var t = 0; t < attentionMask.Length; t++)
        {
            if (attentionMask[t] == 0) continue;
            tokens++;
            var offset = t * dim;
            for (var d = 0; d < dim; d++)
            {
                pooled[d] += hidden[offset + d];
            }
        }

        if (tokens > 0)
        {
            for (var d = 0; d < dim; d++) pooled[d] /= tokens;
        }
        return pooled;
    }

    public static void NormalizeInPlace(float[] vector)
    {
        var norm = 0f;
        foreach (var v in vector) norm += v * v;
        norm = MathF.Sqrt(norm);
        if (norm <= 0f) return;
        for (var i = 0; i < vector.Length; i++) vector[i] /= norm;
    }

    // Truncates a token-id sequence to maxTokens while keeping the trailing
    // special token ([SEP]) intact - cutting it off mid-sequence measurably
    // hurts BERT-family embeddings.
    public static IReadOnlyList<int> Truncate(IReadOnlyList<int> ids, int maxTokens)
    {
        if (ids.Count <= maxTokens) return ids;
        var truncated = new int[maxTokens];
        for (var i = 0; i < maxTokens - 1; i++) truncated[i] = ids[i];
        truncated[maxTokens - 1] = ids[^1];
        return truncated;
    }

    // Pads a batch of variable-length id sequences into rectangular
    // [batch, maxLen] input_ids + attention_mask arrays (row-major), using
    // the given padding id. Returns the padded width.
    public static int PadBatch(
        IReadOnlyList<IReadOnlyList<int>> sequences,
        int padTokenId,
        out long[] inputIds,
        out long[] attentionMask)
    {
        var maxLen = 1;
        foreach (var seq in sequences) maxLen = Math.Max(maxLen, seq.Count);

        inputIds = new long[sequences.Count * maxLen];
        attentionMask = new long[sequences.Count * maxLen];
        for (var row = 0; row < sequences.Count; row++)
        {
            var seq = sequences[row];
            var offset = row * maxLen;
            for (var i = 0; i < maxLen; i++)
            {
                if (i < seq.Count)
                {
                    inputIds[offset + i] = seq[i];
                    attentionMask[offset + i] = 1;
                }
                else
                {
                    inputIds[offset + i] = padTokenId;
                }
            }
        }
        return maxLen;
    }
}
