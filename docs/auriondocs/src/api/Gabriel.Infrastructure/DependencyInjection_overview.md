Registers and configures infrastructure services used by the application's toolset and background features — primarily project file storage, the web page fetcher, and the web search tooling. Call AddInfrastructure on your IServiceCollection during application startup to bind configuration (notably Projects:Files and Tools:Web:Active) and ensure the tool-related HttpClients, search providers, and metrics decorators are available to consumers.

## Remarks
This class centralizes DI wiring for external-facing helpers and tools so callers don't duplicate HttpClient/handler setup, provider selection, or metrics decoration. Web search providers are discovered from the Tools:Web:Active configuration (a comma-separated list of provider keys). Known providers are registered as named HttpClients and concrete IWebSearch implementations; each provider is wrapped with an InstrumentedWebSearch decorator so per-provider call metrics are recorded. If multiple providers are active a CompositeWebSearch merges results (cross-provider hits are ranked first); if no configured provider is recognized the implementation falls back to DuckDuckGo so the tool remains functional instead of failing at first call. The DuckDuckGo client is configured with a long-lived CookieContainer and a longer handler lifetime (about one hour) so session pre-warm cookies are retained across searches.

## Example
```csharp
// In Program.cs / Startup.cs
var builder = WebApplication.CreateBuilder(args);
// Ensure configuration contains Projects:Files and Tools:Web:Active
builder.Services.AddInfrastructure(builder.Configuration);
```

## Notes
- Tools:Web:Active is a comma-separated list of provider keys (e.g. "ddg", "brave", "tavily"); unknown keys are skipped with a warning. The list order does not affect result ranking.
- When only a single provider is active the composite wrapper is bypassed (the provider is still wrapped by the metrics decorator so diagnostics continue to show per-provider events).
- DuckDuckGoHttpClient uses a CookieContainer and an extended handler lifetime so cookies survive across requests; per-request headers (User-Agent rotation, Sec-Fetch-Site, Referer) are applied inside DuckDuckGoWebSearch rather than via DefaultRequestHeaders.
- InstrumentedWebSearch records provider calls to IMetricRecorder and exposes metrics read by IMetricRepository; those metric services are expected to be registered elsewhere (Engine + EF wiring in this project).