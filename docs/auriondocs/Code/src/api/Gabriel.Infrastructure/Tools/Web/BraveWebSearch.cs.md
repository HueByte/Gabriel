# BraveWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/BraveWebSearch.cs`  
> **Kind:** class

```csharp
public sealed class BraveWebSearch : IWebSearch
```


BraveWebSearch is a concrete IWebSearch implementation that queries the Brave Search API via a plain GET to /search with the provided query and a configured API key. It returns a list of WebSearchResult objects mapping Brave results to your domain, and it enforces a small 1–10 result cap and fast fail if the API key isn't configured.

## Remarks
BraveWebSearch isolates the Brave API interaction behind a simple, DI-friendly service. It guards against misconfiguration by throwing when the API key is not configured, ensuring fast feedback during startup or runtime. The class leverages a preconfigured named HttpClient so base address, timeout, and authentication header are defined in one place, reducing repetitive plumbing across the codebase. When the payload is absent or fields are missing, it gracefully falls back to empty values for titles/URLs to preserve a stable surface for consumers.

## Example
```csharp
// Example: usage with DI-provided dependencies
public async Task RunDemo(
    IHttpClientFactory httpFactory,
    IOptions<BraveSearchOptions> options,
    ILogger<BraveWebSearch> logger,
    CancellationToken ct)
{
    var brave = new BraveWebSearch(httpFactory, options, logger);
    var results = await brave.SearchAsync("csharp latest", 3, ct);
    foreach (var r in results)
        Console.WriteLine($"{r.Title} - {r.Url}");
}
```

## Notes
- If Brave's API key is not configured, calling SearchAsync throws InvalidOperationException with guidance on enabling Tools:Web:Brave:ApiKey.
- BraveWebSearch relies on a named HttpClient ("BraveSearch"); ensure your Dependency Injection setup wires BaseAddress, Timeout, and the ApiKey header in one place.
- The limit parameter is clamped to the range 1–10; callers requesting outside values will receive at most ten results.
