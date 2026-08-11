namespace Gabriel.Core.Configuration;

// Embedding provider selection for semantic memory. xAI (the chat provider)
// exposes no embeddings endpoint, so embeddings are a separate concern with
// their own provider + key. "Mock" is a deterministic hashed bag-of-words
// vectorizer that needs no network or key - the default so dev and tests run
// cold; "OpenAI" is the real quality path.
public sealed class EmbeddingOptions : IConfigSection<EmbeddingOptions>
{
    public static string SectionName => "Embeddings";

    // "Mock" | "OpenAI" (case-insensitive).
    public string Provider { get; set; } = "Mock";

    public OpenAIEmbeddingOptions OpenAI { get; set; } = new();

    // Dimension of the mock provider's hashed vectors. Small keeps Qdrant
    // payloads light while still giving meaningful cosine overlap on shared
    // tokens.
    public int MockDimensions { get; set; } = 256;
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
