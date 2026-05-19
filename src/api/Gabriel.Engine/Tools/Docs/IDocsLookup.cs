namespace Gabriel.Engine.Tools.Docs;

// Abstraction over the source of Gabriel's official internal documentation.
// Default implementation pulls from GitHub raw + the git trees API; alternatives
// could read from disk, a docs database, etc. The DocsListTool and DocsReadTool
// in this namespace depend on this interface.
public interface IDocsLookup
{
    Task<IReadOnlyList<DocsEntry>> ListAsync(CancellationToken ct);
    Task<DocsContent?> ReadAsync(string path, CancellationToken ct);
}

// Path is relative to the docs root (e.g. "Gabriel.Engine/architecture.md").
// Title is the first H1 of the doc when available, otherwise null.
public sealed record DocsEntry(string Path, string? Title);

// Returned by ReadAsync. CanonicalUrl is what the docs source thinks the doc's
// permanent URL is (for citations); the tool layer surfaces this to the model.
public sealed record DocsContent(string Path, string Content, string? CanonicalUrl);
