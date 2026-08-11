using System.Net.Http.Json;
using System.Text.Json;
using Gabriel.Core.Configuration;
using Gabriel.Core.Memory;
using Microsoft.Extensions.Options;

namespace Gabriel.Infrastructure.Memory;

// Real embeddings over the OpenAI REST API (xAI - the chat provider - exposes
// no embeddings endpoint, so this is a separate key + provider on purpose).
// Talks plain JSON via a named HttpClient; throws on failure and lets
// QdrantMemoryIndex's non-throwing wrapper decide what degrading looks like.
public sealed class OpenAIEmbeddingProvider : IEmbeddingProvider
{
    public const string HttpClientName = "openai-embeddings";

    private readonly IHttpClientFactory _httpFactory;
    private readonly OpenAIEmbeddingOptions _options;

    public OpenAIEmbeddingProvider(IHttpClientFactory httpFactory, IOptions<EmbeddingOptions> options)
    {
        _httpFactory = httpFactory;
        _options = options.Value.OpenAI;
    }

    public string Name => $"OpenAI:{_options.Model}";
    public int Dimensions => _options.Dimensions;

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        var batch = await EmbedBatchAsync([text], ct);
        return batch[0];
    }

    public async Task<IReadOnlyList<float[]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken ct = default)
    {
        if (texts.Count == 0) return Array.Empty<float[]>();

        var client = _httpFactory.CreateClient(HttpClientName);
        using var response = await client.PostAsJsonAsync(
            "embeddings",
            new { model = _options.Model, input = texts, dimensions = _options.Dimensions },
            ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        // data[] is index-ordered per the API contract, but sort by the
        // explicit index field anyway - the ordering guarantee is theirs to
        // break, the correctness is ours to keep.
        var vectors = new float[texts.Count][];
        foreach (var item in doc.RootElement.GetProperty("data").EnumerateArray())
        {
            var index = item.GetProperty("index").GetInt32();
            var raw = item.GetProperty("embedding");
            var vector = new float[raw.GetArrayLength()];
            var i = 0;
            foreach (var v in raw.EnumerateArray()) vector[i++] = v.GetSingle();
            vectors[index] = vector;
        }

        if (vectors.Any(v => v is null))
            throw new InvalidOperationException("OpenAI embeddings response was missing vectors for some inputs.");
        return vectors;
    }
}
