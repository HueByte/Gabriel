Registers the documentation lookup pipeline and its dependencies into the IServiceCollection. Call this from application startup to configure the local LLM-native docs source, a GitHub-based fallback (including two named HttpClients for API vs raw content), and the CompositeDocsLookup that exposes IDocsLookup with local-first priority.

## Remarks
This method wires two concrete lookup sources and a composite facade: LocalDocsLookup is configured as the primary, LLM-optimized source (reads a local docs folder), and GitHubDocsLookup acts as a fallback that can list via the GitHub API and read raw files from raw.githubusercontent.com. Two named HttpClients are registered so the API and raw endpoints can have different headers/timeouts. The CompositeDocsLookup is registered as the IDocsLookup implementation with the explicit ordering determining priority (local first, then GitHub). Configuration for both sources is bound from IConfiguration sections.

## Example
```csharp
// In Startup.ConfigureServices or Program.cs (minimal host):
public void ConfigureServices(IServiceCollection services)
{
    // other service registrations...
    AddDocsLookup(services, Configuration);
}

// Or in Program.cs with the WebApplicationBuilder:
var builder = WebApplication.CreateBuilder(args);
AddDocsLookup(builder.Services, builder.Configuration);
```

## Notes
- The order of the IDocsLookup instances determines lookup priority; local docs are intentionally added first so they shadow GitHub content.
- GitHub HTTP clients use a 15s timeout and add an Authorization header only when a token is present in configuration; providing a token reduces rate-limit issues.
- Services are registered as singletons; ensure any dependencies used by LocalDocsLookup/GitHubDocsLookup are safe for singleton usage.
- CompositeDocsLookup is resolved with an ILogger; the logging provider must be available in DI (normally added by the host).