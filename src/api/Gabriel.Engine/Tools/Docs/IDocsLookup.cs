namespace Gabriel.Engine.Tools.Docs;

// Abstraction over a source of Gabriel's official internal documentation.
// Implementations include LocalDocsLookup (LLM-native folder on disk - the
// PRIMARY source) and GitHubDocsLookup (the human-prose docs on GitHub - the
// fallback). CompositeDocsLookup fans across an ordered list of inner sources
// to give the docs tools a single facade.
public interface IDocsLookup
{
    Task<IReadOnlyList<DocsEntry>> ListAsync(CancellationToken ct);
    Task<DocsContent?> ReadAsync(string path, CancellationToken ct);
}

// Path is relative to the source's own root (e.g. "README.md" for the local
// LLM-native source; "Gabriel.Engine/architecture.md" for the GitHub source).
// Title is the first H1 of the doc when available, otherwise null.
// Source is a free-form tag identifying where the entry came from so the tool
// layer can surface it ("local-llm-native" / "github").
public sealed record DocsEntry(string Path, string? Title, string Source);

// Returned by ReadAsync. CanonicalUrl is what the source thinks the doc's
// permanent URL is (for citations / links) - for the local source this is a
// file:// URI, for GitHub it's the github.com blob URL.
public sealed record DocsContent(string Path, string Content, string? CanonicalUrl, string Source);

public static class DocsSources
{
    public const string LocalLlmNative = "local-llm-native";
    public const string GitHub = "github";
}
