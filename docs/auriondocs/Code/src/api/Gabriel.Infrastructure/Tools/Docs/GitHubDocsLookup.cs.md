# GitHubDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/Tools/Docs/GitHubDocsLookup.cs`  
> **Kind:** class

```csharp
public sealed class GitHubDocsLookup : IDocsLookup
```


GitHubDocsLookup is an IDocsLookup implementation that sources documentation entries from a GitHub repository. It exposes two transport paths: a listing path that enumerates the repository tree to discover Markdown docs, and a read path that fetches individual documents directly from GitHub Raw. Listings are cached for up to ListCacheMinutes to respect unauthenticated rate limits; reads pass through live content, which may reflect edits made in the repository during development. Path traversal is hardened: any '..' segment or absolute-prefixed path is rejected before the request is sent.

## Remarks

GitHubDocsLookup decouples the source of docs from consumers by implementing IDocsLookup, allowing swapping the backend without changing callers. The two-phase approach separates cataloging (ListAsync) from content retrieval (ReadAsync), enabling a stable UI to present available docs while ensuring current content is fetched on demand. Caching and a semaphore ensure thread-safe, rate-limit-friendly operation under concurrent load.

## Notes

- Configure HttpClientFactory with two named clients: GitHubDocsApi for the trees API and GitHubDocsRaw for raw file retrieval; these can be tuned independently.
- The implementation explicitly rejects traversal-unsafe paths to prevent directory traversal attacks.
- On failure to the Trees API, a HttpRequestException is thrown after logging the response body; callers should handle this as an operational error.