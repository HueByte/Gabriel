# GitHubDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/Tools/Docs/GitHubDocsLookup.cs`  
> **Kind:** class

Provides an IDocsLookup implementation that lists and reads Markdown documentation files from a GitHub repository. Use this when your documentation is stored in a GitHub repo and you need a runtime lookup that enumerates .md files via the GitHub Trees API and fetches file contents from raw.githubusercontent.

## Remarks
GitHubDocsLookup uses two HTTP transport paths: the GitHub Trees API (via the named client "GitHubDocsApi") to produce a repository listing, and the raw.githubusercontent URL (via the named client "GitHubDocsRaw") to read individual files. The list response is cached for the duration configured in GitHubDocsOptions.ListCacheMinutes to reduce unauthenticated API requests; listing is protected by a SemaphoreSlim so only one refresh runs at a time. Read operations are not cached (they go directly to the raw URL) so live edits in the repo are reflected immediately. Path traversal is validated and rejected early: any absolute path or path containing ".." is refused before an outbound request is made.

## Example
```csharp
// Resolve an IDocsLookup (e.g. from DI) and use it to list and read docs.
IReadOnlyList<DocsEntry> entries = await lookup.ListAsync(cancellationToken);
// pick an entry path from entries, then read its content
DocsContent? content = await lookup.ReadAsync("getting-started/intro.md", cancellationToken);
```

## Notes
- Configure IHttpClientFactory with the named clients used by this class: "GitHubDocsApi" should have a BaseAddress of https://api.github.com/ (and appropriate headers like User-Agent); "GitHubDocsRaw" can be left without a BaseAddress because reads use an absolute raw.githubusercontent.com URL.
- ListAsync only returns entries for files ending with .md and sets entry content to null (it does not prefetch file bodies). ReadAsync performs the actual file fetch and returns the contents.
- If the GitHub API returns a non-success status, ListAsync logs a warning and throws an HttpRequestException. Invalid or unsafe paths passed to ReadAsync result in an ArgumentException before any HTTP call is made.