# GitHubDocsOptions

> **File:** `src/api/Gabriel.Core/Configuration/GitHubDocsOptions.cs`  
> **Kind:** class

Represents configuration for the GitHub-backed documentation source used by the docs tool. Reach for this when you need to point the docs importer at a different GitHub repository, branch, path, or to supply credentials; defaults are set so the tool works without any configuration.

## Remarks
This POCO is intended to be bound from the "Tools:Docs:GitHub" configuration section (see the SectionName property) and consumed by the docs tooling to locate and fetch markdown files. Defaults target the canonical upstream repository so no configuration is required for typical read-only use; provide a personal access token only if you need higher GitHub API rate limits or access to private repos. The ListCacheMinutes value controls caching of list responses (file listings), while individual file reads are intentionally not cached.

## Example
```csharp
// appsettings.json
{
  "Tools": {
    "Docs": {
      "GitHub": {
        "Owner": "HueByte",
        "Repo": "Gabriel",
        "Branch": "main",
        "DocsPath": "docs",
        "ListCacheMinutes": 5
      }
    }
  }
}

// Environment variable to set the token (example for CI/secret manager integration):
// TOOLS__DOCS__GITHUB__TOKEN
```

## Notes
- Unauthenticated GitHub API access is rate-limited (~60 requests/hour per IP); supplying a PAT via Token raises this to ~5000 requests/hour.
- ListCacheMinutes only affects caching of directory/list responses; file reads are left uncached to reflect in-flight edits.
- DocsPath is used as a prefix: the tool walks markdown (.md) files under this path only.