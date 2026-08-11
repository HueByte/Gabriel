# GitHubDocsOptions

> **File:** `src/api/Gabriel.Core/Configuration/GitHubDocsOptions.cs`  
> **Kind:** class

```csharp
public class GitHubDocsOptions : IConfigSection<GitHubDocsOptions>
```


GitHubDocsOptions is a strongly-typed configuration section that drives how the docs tool fetches Markdown files from GitHub. It encapsulates the repository identity (Owner, Repo), the target Branch, the path within the repo where docs live, an optional authentication token, and a cache duration for directory listings, enabling consistent, configurable access instead of scattered literals.

## Remarks
As a configuration anchor, it implements [`IConfigSection<GitHubDocsOptions>`](IConfigSection.cs.md) to integrate with the application's configuration pipeline. The static SectionName identifies the key under which these settings are defined, allowing consumers to bind a GitHubDocsOptions instance from configuration sources. The defaults point at the canonical upstream repository, which makes the docs tool usable out of the box while still permitting project-specific overrides.

## Notes
- SectionName is the config key used by the configuration system to locate this section (Tools:Docs:GitHub).
- Token is optional; when provided, authenticated GitHub API requests bypass the unauthenticated rate-limit and can support higher request throughput.
- ListCacheMinutes controls how long the repository's directory listings are cached; reads are not cached and may reflect in-flight edits to docs.