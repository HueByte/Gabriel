# GitHubDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/Tools/Docs/GitHubDocsLookup.cs`  
> **Kind:** class

```csharp
public sealed class GitHubDocsLookup : IDocsLookup
```


GitHubDocsLookup implements IDocsLookup by serving documentation stored in a GitHub repository. It offers two transport paths: ListAsync, which enumerates Markdown docs by querying the repository tree, and ReadAsync, which fetches raw Markdown content from GitHub's raw endpoint. The implementation caches the list for a configurable window to reduce unauthenticated API usage and hardens path traversal to reject any unsafe paths before requesting data.

## Remarks
Conceptually, it decouples docs from the rest of the system and provides a single, testable path to locate and retrieve documentation assets. It uses an HttpClientFactory to perform HTTP calls, and an options object to determine ownership, repository, branch, and the path under which docs live. A semaphore guarantees only one refresh of the listing happens at a time, preventing thundering herds. The path validation comment in the source highlights a deliberate boundary: only files under the configured DocsPath with a .md extension are considered, mitigating accidental exposure of non-document assets.

## Notes
- The ListAsync cache is time-bounded by ListCacheMinutes; if the cache expires or is empty, the listing will re-fetch from GitHub.
- Non-success responses from the Trees API trigger a warning log and throw HttpRequestException, so callers should handle errors gracefully.
- ReadAsync uses an absolute URL to raw.githubusercontent.com, which overrides any BaseAddress on the configured HttpClient; this ensures the request targets the intended raw docs location regardless of client defaults.