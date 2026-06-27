# GitHubDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/Tools/Docs/GitHubDocsLookup.cs`  
> **Kind:** class

Provides an IDocsLookup implementation that reads documentation files from a GitHub repository. Use this when you want to surface markdown docs stored in a GitHub repo (configured via GitHubDocsOptions) — the class lists available .md files using the GitHub Trees API and reads file contents from raw.githubusercontent.com.

## Remarks
This implementation balances rate-limit concerns and live-editability: the repository tree (the list of docs) is cached for a short, configurable period to reduce unauthenticated API requests, while individual document reads are not cached so editors can update files and see changes immediately. ListAsync is concurrency-safe — it uses a semaphore to ensure only one fetch populates the cache at a time. The lookup filters results to .md files under the configured DocsPath and returns DocsEntry items with DocsSources.GitHub.

## Example
```csharp
// Resolve via DI (IHttpClientFactory and named HttpClients must be configured):
var lookup = serviceProvider.GetRequiredService<IDocsLookup>();
var ct = CancellationToken.None;

// Get the list of markdown docs (cached for ListCacheMinutes):
var entries = await lookup.ListAsync(ct);

// Read a specific document (no cache; fetched from raw.githubusercontent.com):
if (entries.Count > 0)
{
    var content = await lookup.ReadAsync(entries[0].Path, ct);
    // content may be null or contain the document body and metadata
}
```

## Notes
- The class expects two named HttpClient registrations: "GitHubDocsApi" (used for the Trees API) and "GitHubDocsRaw" (used for raw file reads). The raw read uses an absolute raw.githubusercontent.com URL and ignores any BaseAddress on the named client.
- Path validation is enforced: paths containing traversal segments (e.g. "..") or absolute-prefixed paths are rejected (ArgumentException) before any network request is issued.
- ListAsync throws an HttpRequestException when the GitHub Trees API responds with a non-success status; the response body is logged for diagnostics.
- Only files under the configured DocsPath with a ".md" extension are returned by ListAsync.
- List results are cached for GitHubDocsOptions.ListCacheMinutes (default is short — intended to keep unauthenticated request counts low); individual reads are intentionally not cached so edits are visible immediately.
