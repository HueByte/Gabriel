Registers the documentation lookup infrastructure into the dependency-injection container. Call this from application startup (ConfigureServices / Program.cs) when you want the app to resolve an IDocsLookup that first consults a local LLM-focused docs folder and falls back to GitHub-hosted documentation.

## Remarks
This method wires two concrete docs sources and a composite facade: LocalDocsLookup (primary source, reads the LLM-native docs directory) and GitHubDocsLookup (fallback, reads GitHub API for listing and raw.githubusercontent for content). GitHubDocsLookup is configured with two named HttpClient instances — one tuned for the GitHub JSON API and one for raw content — and the composite preserves priority order (local first) while deduplicating entries by path. A failing source is isolated so one backend's transient errors won't prevent others from answering queries.

## Example
```csharp
var builder = WebApplication.CreateBuilder(args);
// registers LocalDocsOptions and GitHubDocsOptions from configuration,
// the GitHub named HttpClients, and the Composite IDocsLookup.
builder.Services.AddDocsLookup(builder.Configuration);
var app = builder.Build();
```

## Notes
- Ensure LocalDocsOptions.SectionName points to a valid local docs folder; missing local pages will cause lookups to fall back to GitHub.
- If GitHubDocsOptions.Token is not provided, requests are unauthenticated and may be subject to stricter rate limits; when provided the token is sent as a Bearer Authorization header.
- The code registers two named HttpClients (ApiHttpClientName and RawHttpClientName) with specific headers and timeouts; other code that depends on GitHubDocsLookup expects those client names to exist.