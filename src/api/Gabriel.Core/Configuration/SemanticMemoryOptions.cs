namespace Gabriel.Core.Configuration;

// Semantic memory (Qdrant-backed vector recall) configuration. Disabled by
// default so the app runs with zero external dependencies; when disabled the
// agent falls back to injecting every saved memory verbatim (pre-Qdrant
// behavior).
public sealed class SemanticMemoryOptions : IConfigSection<SemanticMemoryOptions>
{
    public static string SectionName => "SemanticMemory";

    public bool Enabled { get; set; }

    // Qdrant REST endpoint. The official image exposes REST on 6333; we speak
    // REST (not gRPC) to keep the dependency surface at HttpClient.
    public string QdrantUrl { get; set; } = "http://localhost:6333";

    // Optional API key sent as the "api-key" header (Qdrant cloud / secured
    // deployments). Empty for a local unsecured container.
    public string QdrantApiKey { get; set; } = string.Empty;

    public string Collection { get; set; } = "gabriel_memories";

    public int TimeoutSeconds { get; set; } = 10;

    // How many semantic hits get their full body injected into the turn
    // context (the rest of the memories appear as a name+description index).
    public int RecallTopK { get; set; } = 5;

    // Cosine-similarity floor for recall. Real-text embeddings (e.g.
    // text-embedding-3-small) put related pairs around 0.3-0.6, so the
    // default is deliberately permissive; raise it if recall gets noisy.
    public float RecallMinScore { get; set; } = 0.35f;
}
