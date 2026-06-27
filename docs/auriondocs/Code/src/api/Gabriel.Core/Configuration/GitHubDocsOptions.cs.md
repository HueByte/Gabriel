# GitHubDocsOptions

> **File:** `src/api/Gabriel.Core/Configuration/GitHubDocsOptions.cs`  
> **Kind:** class

Configuration for the GitHub-backed documentation source used by the docs tooling. Bind this section (Tools:Docs:GitHub) into your application's configuration system or options pipeline when the docs tool should read markdown files from a GitHub repository; it centralizes repository location, branch, docs path, optional access token, and a small cache tuning value.

## Remarks
This type provides safe defaults so the docs tool works without additional configuration (defaults point at the canonical upstream repository). The SectionName constant is the configuration path used to bind these values. The Token is optional — unauthenticated API usage is allowed but rate-limited; supplying a personal access token raises the GitHub API limit. ListCacheMinutes controls caching of repository listing responses only; individual file reads are intentionally not cached.

## Example
```csharp
// appsettings.json (excerpt)
// {
//   "Tools": {
//     "Docs": {
//       "GitHub": {
//         "Owner": "HueByte",
//         "Repo": "Gabriel",
//         "Branch": "main",
//         "DocsPath": "docs",
//         "Token": null,
//         "ListCacheMinutes": 5
//       }
//     }
//   }
// }

// Register the options in DI
builder.Services.Configure<GitHubDocsOptions>(
    builder.Configuration.GetSection(GitHubDocsOptions.SectionName)
);

// Or read the options directly
var opts = builder.Configuration
    .GetSection(GitHubDocsOptions.SectionName)
    .Get<GitHubDocsOptions>();
```

## Notes
- Token is nullable; supply a personal access token via configuration or the environment variable name shown in comments (e.g. TOOLS__DOCS__GITHUB__TOKEN) if you need higher GitHub API rate limits.
- Defaults target the upstream repository; the local working-tree folder name may differ — the remote repo name (Owner + Repo) is what the tool uses.
- ListCacheMinutes only affects caching of list operations (e.g. directory listings); file content reads are not cached by this setting.