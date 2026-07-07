# GitHubDocsOptions

> **File:** `src/api/Gabriel.Core/Configuration/GitHubDocsOptions.cs`  
> **Kind:** class

```csharp
public class GitHubDocsOptions : IConfigSection<GitHubDocsOptions>
```


GitHubDocsOptions is a configuration container used by the documentation tooling to locate and fetch Markdown-based documentation from a GitHub repository. It implements [`IConfigSection<GitHubDocsOptions>`](IConfigSection.cs.md), enabling it to be loaded from a shared configuration source. It exposes the repository details (Owner, Repo, Branch), the path within the repository where Markdown docs live (DocsPath), and optional authentication via a Personal Access Token (Token). The ListCacheMinutes setting controls how long the docs list responses are cached, ensuring responsive lookups while keeping edits fresh.

## Remarks
At a conceptual level, this type abstracts away the specifics of a GitHub-backed docs store from the rest of the toolchain. It centralizes defaults that align with the upstream canonical repo, while allowing overrides per deployment. Because it implements IConfigSection, it participates in the configuration lifecycle and can be validated alongside other config sections. Practically, components that need to read docs can rely on this strongly-typed configuration object rather than ad-hoc strings or scattered literals.

## Notes
- List caching applies to list responses; individual document reads are not cached to reflect in-flight edits promptly.
- Token is optional; if omitted, the tooling may hit GitHub's unauthenticated rate limits. Provide a Personal Access Token via Token to increase available request quotas when performing heavy lookups.