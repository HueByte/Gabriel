Registers infrastructure-layer services used by the application and tools. This class provides extension-based wiring for project file storage, the web page fetcher used by the web_fetch tool, and pluggable web search providers; it binds options from configuration and composes per-provider and composite search implementations so callers can simply call AddInfrastructure on their IServiceCollection.

## Remarks
This is the central composition point for infrastructure concerns that live outside the core engine: HTTP-based fetch/search tooling and disk-backed project file storage. It reads configuration (e.g. Tools:Web:Active and Projects:Files), registers named HttpClient instances with handler lifetimes and cookie handling tuned for real browsers, and wraps concrete IWebSearch implementations with an InstrumentedWebSearch decorator so per-provider calls are recorded to the generic IMetricRecorder/IMetricRepository plumbing already provided by the engine/EF wiring. The class also establishes a safe fallback (DuckDuckGo) when configuration is missing or invalid so the toolset remains operational rather than throwing at runtime.

## Example
```csharp
// In Program.cs / Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // IConfiguration `config` is typically available from the host builder
    services.AddInfrastructure(Configuration);
}
```

## Notes
- Web search configuration: Tools:Web:Active is a comma-separated list of provider keys (e.g. "ddg", "brave", "tavily"); unknown keys are ignored with a warning and an empty or entirely-unrecognized configuration falls back to DuckDuckGo.
- Per-provider call recording is performed by InstrumentedWebSearch (a decorator) which writes into IMetricRecorder; this requires the engine/EF metric services to be registered earlier.
- DuckDuckGo HttpClient registration uses a long-lived CookieContainer and an increased handler lifetime (about an hour) so session cookies from homepage pre-warm persist across searches; DuckDuckGo-specific per-request headers (User-Agent rotation, Sec-Fetch-Site, Referer) are applied by DuckDuckGoWebSearch per HttpRequestMessage rather than via DefaultRequestHeaders.
- The web fetcher uses a single HttpClient with a sensible timeout and a normal browser User-Agent because many sites reject blank or script-like UAs; redirects are allowed and an SSRF guard evaluates the final destination via request hooks.
- Because unknown/empty provider lists are tolerated (with a fallback), configuration typos will not crash the application but may change which provider is actually used.