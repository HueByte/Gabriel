namespace Gabriel.Core.Memory;

// Turns text into a dense vector for semantic search. Implementations live in
// Gabriel.Infrastructure (OpenAI HTTP, deterministic mock); the interface sits
// in Core so MemoryService and the Engine can depend on it without knowing the
// transport. Dimensions must be constant for the lifetime of a collection -
// changing embedding models requires a reindex into a fresh collection.
public interface IEmbeddingProvider
{
    string Name { get; }
    int Dimensions { get; }

    Task<float[]> EmbedAsync(string text, CancellationToken ct = default);

    // Batch variant so reindex jobs don't pay one HTTP round-trip per memory.
    Task<IReadOnlyList<float[]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken ct = default);
}
