using System.Security.Cryptography;
using System.Text;
using Gabriel.Core.Configuration;
using Gabriel.Core.Memory;
using Microsoft.Extensions.Options;

namespace Gabriel.Infrastructure.Memory;

// Deterministic, network-free embeddings: a hashed bag-of-words. Each token is
// SHA256-hashed to a stable bucket + sign, counts accumulate, and the vector
// is L2-normalized so cosine similarity reflects token overlap. It is not
// semantically smart - it exists so semantic memory runs end-to-end (dev,
// tests, CI, Docker) without an embeddings API key, mirroring how
// MockChatProvider keeps the chat loop alive without an xAI key.
public sealed class MockEmbeddingProvider : IEmbeddingProvider
{
    private readonly int _dimensions;

    public MockEmbeddingProvider(IOptions<EmbeddingOptions> options)
    {
        // Floor of 16 keeps degenerate configs from collapsing every token
        // into a handful of buckets (which would make everything similar).
        _dimensions = Math.Max(16, options.Value.MockDimensions);
    }

    public string Name => "Mock";
    public int Dimensions => _dimensions;

    public Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
        => Task.FromResult(Embed(text));

    public Task<IReadOnlyList<float[]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<float[]>>(texts.Select(Embed).ToList());

    private float[] Embed(string text)
    {
        var vector = new float[_dimensions];
        foreach (var token in Tokenize(text))
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            var bucket = (int)(BitConverter.ToUInt32(hash, 0) % (uint)_dimensions);
            // A stable per-token sign spreads mass across both directions,
            // which stops unrelated texts from all scoring positive just by
            // having lots of tokens.
            var sign = (hash[4] & 1) == 0 ? 1f : -1f;
            vector[bucket] += sign;
        }

        var norm = 0f;
        foreach (var v in vector) norm += v * v;
        norm = MathF.Sqrt(norm);
        if (norm > 0)
        {
            for (var i = 0; i < vector.Length; i++) vector[i] /= norm;
        }
        return vector;
    }

    private static IEnumerable<string> Tokenize(string text)
    {
        var sb = new StringBuilder();
        foreach (var ch in text)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(char.ToLowerInvariant(ch));
            }
            else if (sb.Length > 0)
            {
                yield return sb.ToString();
                sb.Clear();
            }
        }
        if (sb.Length > 0) yield return sb.ToString();
    }
}
