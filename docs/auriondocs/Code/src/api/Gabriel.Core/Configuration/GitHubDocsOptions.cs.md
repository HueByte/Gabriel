# GitHubDocsOptions

> **File:** `src/api/Gabriel.Core/Configuration/GitHubDocsOptions.cs`  
> **Kind:** class

```csharp
public class GitHubDocsOptions : IConfigSection<GitHubDocsOptions>
```


GitHubDocsOptions is a configuration container for the docs tooling that fetches project documentation from a GitHub repository. It exposes the repository owner, repository name, branch, the path within the repo where docs live, an optional authentication token, and a cache duration for the list of docs, all with sensible defaults to enable out-of-the-box operation.

## Remarks
GitHubDocsOptions exists to decouple the docs lookup logic from hard-coded details and to support testing and multi-environment configurations. By implementing [`IConfigSection<GitHubDocsOptions>`](IConfigSection.cs.md) and exposing a static SectionName, it participates in the application's configuration binding and allows values to be supplied from config sources (JSON, environment variables, etc.) without changing code. The Token property is intended for secure credentials, and can be provided via a secret store or environment variable to raise GitHub API rate limits when needed.

## Notes
- Unauthenticated requests are rate-limited by GitHub; provide a Token to increase the allowed request quota for docs operations.
- ListCacheMinutes governs caching of the repository's docs list; changes to docs may not be reflected immediately due to caching, so adjust ListCacheMinutes accordingly if freshness is critical.