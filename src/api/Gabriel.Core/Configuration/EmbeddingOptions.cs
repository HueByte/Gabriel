namespace Gabriel.Core.Configuration;

// Embedding provider selection for semantic memory. xAI (the chat provider)
// exposes no embeddings endpoint, so embeddings are a separate concern.
// Three providers, no vendor lock-in:
//   "Local"  - open-source sentence-transformer run in-process via ONNX
//              Runtime (default model: all-MiniLM-L6-v2, Apache-2.0, 384-dim,
//              ~90 MB on disk). Real semantic quality, no API key, no network.
//              Needs the model files on disk - scripts/download-embedding-model.ps1.
//   "OpenAI" - hosted text-embedding-3-small; highest quality, needs a key.
//   "Mock"   - deterministic hashed bag-of-words; no files, no key. The
//              cold-start default so dev/tests/CI run with zero setup.
// A provider whose prerequisites are missing (no model files, no key) falls
// back to Mock at DI time rather than failing startup.
public sealed class EmbeddingOptions : IConfigSection<EmbeddingOptions>
{
    public static string SectionName => "Embeddings";

    // "Mock" | "Local" | "OpenAI" (case-insensitive).
    public string Provider { get; set; } = "Mock";

    public LocalEmbeddingOptions Local { get; set; } = new();

    public OpenAIEmbeddingOptions OpenAI { get; set; } = new();

    // Dimension of the mock provider's hashed vectors. Small keeps Qdrant
    // payloads light while still giving meaningful cosine overlap on shared
    // tokens.
    public int MockDimensions { get; set; } = 256;
}

public sealed class LocalEmbeddingOptions
{
    // Directory holding the ONNX model + vocab. Relative paths are probed
    // from the working directory and AppContext.BaseDirectory plus a few
    // parents (same walk-up idea as LocalDocsLookup), so `dotnet run` from
    // src/api and the Docker image both resolve without config surgery.
    public string ModelDirectory { get; set; } = "models/embeddings/all-MiniLM-L6-v2";

    public string ModelFile { get; set; } = "model.onnx";

    // BERT WordPiece vocabulary (one token per line).
    public string VocabFile { get; set; } = "vocab.txt";

    // Must match the model's hidden size (all-MiniLM-L6-v2 = 384). Verified
    // against the actual model output at inference time; a mismatch throws a
    // descriptive error instead of silently indexing garbage.
    public int Dimensions { get; set; } = 384;

    // Token cap per input (model position limit is 512; memories are short,
    // 256 keeps latency down). Longer inputs are truncated, keeping [SEP].
    public int MaxTokens { get; set; } = 256;
}

public sealed class OpenAIEmbeddingOptions
{
    // Trailing slash matters: BaseAddress + "embeddings" must land on
    // /v1/embeddings.
    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";

    // Supply via user-secrets / env (EMBEDDINGS__OPENAI__APIKEY). Empty =
    // unconfigured; selecting Provider=OpenAI without a key falls back to a
    // disabled index rather than crashing at startup.
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "text-embedding-3-small";

    public int Dimensions { get; set; } = 1536;

    public int TimeoutSeconds { get; set; } = 30;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
