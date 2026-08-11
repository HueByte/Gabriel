# GitHubDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/Tools/Docs/GitHubDocsLookup.cs`  
> **Kind:** class

```csharp
public sealed class GitHubDocsLookup : IDocsLookup
```


GitHubDocsLookup is a concrete implementation of IDocsLookup that fetches docs from a GitHub repository. It uses two transport paths—listing the repository tree via the GitHub API and reading individual docs from raw.githubusercontent.com—and caches the list of entries for a configured interval; path validation and a semaphore ensure safe, efficient operation under concurrent access.

## Remarks

GitHubDocsLookup abstracts GitHub-specific access behind the IDocsLookup interface, letting the rest of the system discover and retrieve docs without caring about transport or cache details. When listing, it queries the recursive tree, filters for markdown blobs under the configured DocsPath, and returns DocsEntry items marked as GitHub sources. Reading a doc validates the path, builds an absolute URL to raw.githubusercontent.com, and fetches the document content via a dedicated HTTP client. The implementation relies on two named HttpClients (GitHubDocsApi and GitHubDocsRaw) and a configurable ListCacheMinutes to balance freshness with rate-limiting constraints.

## Notes

- List cache is controlled by GitHubDocsOptions.ListCacheMinutes and guarded by a semaphore to serialize refreshes.
- ValidatePath(path) protects against '..' segments and absolute paths before any request.
- On a non-successful API response during ListAsync, a warning is logged and an HttpRequestException is thrown to surface the failure to callers.