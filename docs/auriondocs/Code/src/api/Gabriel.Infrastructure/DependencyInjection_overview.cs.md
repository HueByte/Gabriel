Adds infrastructure-level services used across the application: file-backed project storage, an HTTP-based web fetcher, and configurable web-search provider wiring. Use this extension on IServiceCollection during application startup to register the concrete implementations, named HttpClients, and the metrics decorators that the tools (web_fetch, web_search, etc.) expect.

## Remarks
This class centralizes DI registration for cross-cutting infra concerns so callers get consistent HttpClient configuration, handler lifetimes and decorator wiring in one place. Web-search registration supports a comma-separated Tools:Web:Active configuration that can enable one or many providers; the code builds either a single provider registration or a CompositeWebSearch that merges results and preserves per-provider metrics via InstrumentedWebSearch. DuckDuckGo is used as the safe default when configuration is empty or contains only unrecognized keys. The DuckDuckGo HttpClient uses a long-lived CookieContainer and an extended handler lifetime so session cookies persist across requests; per-request headers (User-Agent rotation, Sec-Fetch-Site, Referer) are applied at request time in the DuckDuckGoWebSearch implementation rather than via DefaultRequestHeaders.

## Example
```csharp
// In Program.cs or Startup.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
```

## Notes
- A typo or unrecognized provider key in Tools:Web:Active is ignored (logged as a warning) — an empty resulting set falls back to DuckDuckGo so the tool continues to function.
- DuckDuckGoWebSearch relies on a shared CookieContainer and an increased handler lifetime (≈1 hour) to avoid losing session state mid-conversation; do not re-create the handler frequently.
- InstrumentedWebSearch decorates providers and records per-provider call metrics (IMetricRecorder / IMetricRepository must be available; Engine/EF wiring typically provides these).