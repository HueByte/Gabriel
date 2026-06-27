Registers the documentation lookup pipeline into the DI container so the runtime can resolve IDocsLookup. Use this during application startup/service registration when you want the application to consult a local LLM-optimized docs folder first and fall back to GitHub-hosted documentation if a topic is not covered locally.

## Remarks
This method wires three implementations into the container: LocalDocsLookup (primary source), GitHubDocsLookup (fallback), and a CompositeDocsLookup that composes them in priority order. It configures options for both local and GitHub sources, creates two named HttpClients for the GitHub implementation (one for the JSON API and one for raw content), and registers the lookups as singletons. The composite lookup performs unioned listing (primary entries first, deduplicated) and short-circuits reads on the first successful source; a failing source does not prevent the composite from trying the others.

## Example
```csharp
// In Program.cs or Startup.cs during service registration
public void ConfigureServices(IServiceCollection services)
{
    // configuration is an IConfiguration instance already built
    AddDocsLookup(services, Configuration);

    // other registrations...
}
```

## Notes
- Ensure configuration sections for LocalDocsOptions and GitHubDocsOptions are present; the method binds those sections via IConfiguration.
- GitHubDocsLookup uses two named HttpClients: one for the GitHub API (list operations) and one for raw content (read operations); both have a 15s timeout by default.
- Providing a GitHub token in GitHubDocsOptions avoids unauthenticated rate limits; the token is optional but recommended for production workloads.