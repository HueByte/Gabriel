# DependencyInjection.cs

> **Source:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`

## Contents

- [DependencyInjection_overview](#dependencyinjection_overview)
- [AddChatProvider](#addchatprovider)
- [AddDocsLookup](#adddocslookup)
- [AddInfrastructure](#addinfrastructure)
- [AddWebFetch](#addwebfetch)
- [AddWebSearch](#addwebsearch)
- [ConfigureDdgHttpClient](#configureddghttpclient)
- [ConfigureGrokResilience](#configuregrokresilience)

---

## DependencyInjection_overview

> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** class

Registers the application's infrastructure services and tools into an IServiceCollection. Use this when bootstrapping the application (typically in Program.cs or Startup) to add project file storage, the web-fetching tool, web search providers and their HttpClient wiring, and related instrumentation. It reads configuration (e.g. Projects:Files and Tools:Web:* keys) to selectively enable and configure providers.

## Remarks
This static class centralizes DI wiring for infrastructure concerns so the rest of the codebase can depend on stable abstractions (IWebSearch, web-fetch tool, file storage) without knowing provider details. The web search registration supports multiple providers configured via Tools:Web:Active (comma-separated keys); single-provider setups bypass the composite wrapper for lower overhead, while multi-provider setups use a CompositeWebSearch that merges and ranks results. Each provider is wrapped with an InstrumentedWebSearch decorator so per-provider call metrics are recorded (it expects IMetricRecorder / IMetricRepository to be available from higher-level registrations). DuckDuckGo's HttpClient is configured with a long-lived CookieContainer and extended handler lifetime to preserve session cookies between requests; per-request headers and UA rotation are applied by the concrete DuckDuckGoWebSearch implementation.

## Example
```csharp
// Typical Program.cs / Startup usage
var builder = WebApplication.CreateBuilder(args);
// configuration already loaded into builder.Configuration
builder.Services.AddInfrastructure(builder.Configuration);
var app = builder.Build();
```

## Notes
- Tools:Web:Active is a comma-separated list of provider keys (e.g. "ddg", "brave", "tavily"). Unknown keys are ignored and a missing/empty configured set falls back to DuckDuckGo so the tool continues to work.
- Instrumented metrics require the metric services (IMetricRecorder/IMetricRepository) to be registered by the Engine/EF wiring before they can be used for per-provider call recording.
- DuckDuckGo's HttpClient uses a shared CookieContainer and extended handler lifetime (≈1 hour) — be aware this increases resource lifetime compared to default HttpClientHandler settings.
- The web-fetch client is configured with a browser-like User-Agent and a sensible timeout; many sites reject blank or script-like UAs, so do not remove or replace the UA without testing.


---

## AddChatProvider

> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

Registers chat provider implementations and any related HTTP/option plumbing into the provided IServiceCollection.

This method always registers a MockChatProvider as a singleton (a safe fallback so the agent layer never has zero providers) and conditionally wires up the Grok provider when the Providers:Grok configuration section exists and contains at least one model. When Grok is enabled it binds and validates GrokOptions via the Options pipeline (API key, BaseUrl, timeouts, model list constraints), registers the Grok auth DelegatingHandler as transient, and configures a named HttpClient and resilience/timeout settings used by GrokChatProvider.

## Remarks
AddChatProvider centralizes provider registration and configuration validation so the rest of the application can depend on IChatProvider without needing to know which concrete providers are present. The method enforces configuration invariants (required API key, valid BaseUrl, a positive timeout, model entries with required fields, and at most one default model) early via Options validation. It also captures a provider-level timeout at startup (used by the resilience pipeline) and ensures the auth handler uses a transient lifetime so HttpClientFactory can add it as a message handler.

## Example
```csharp
// Typical call from Program.cs or a startup composition root
public void ConfigureServices(IServiceCollection services)
{
    IConfiguration config = /* built configuration */;
    AddChatProvider(services, config);
}
```

## Notes
- The Grok options validators run at startup when ValidateOnStart is enabled; this is skipped when the environment variable SKIP_DB_INIT is set to "true" to accommodate processes (like codegen) that don't have secrets available.
- The Grok API key is expected in the provider section (examples: env var PROVIDERS__GROK__APIKEY or a secret manager such as Infisical as noted in comments).
- The Grok DelegatingHandler is registered transient because handlers added with AddHttpMessageHandler must be transient to work correctly with IHttpClientFactory.
- The configured named HttpClient leaves HttpClient.Timeout unset (effectively infinite) so the library's resilience pipeline (configured separately) is the only timeout authority; the code captures TimeoutSeconds at startup for that pipeline.

---

## AddDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

Registers the documentation lookup stack into the application's dependency injection container: a primary LocalDocsLookup, a GitHub-based fallback (with two named HttpClients for API and raw content), and a CompositeDocsLookup exposed as IDocsLookup. Use this during application startup to wire the docs sources so consumers can ListAsync and ReadAsync documentation without caring about the underlying providers.

## Remarks
This method centralizes the wiring and configuration required to support two-tier documentation lookup: a local, LLM-friendly docs folder (primary) and a GitHub raw/docs fallback. It binds configuration sections to LocalDocsOptions and GitHubDocsOptions, registers two named HttpClients with sensible defaults and headers (including optional bearer token support for the GitHub API), and composes the two lookups into a CompositeDocsLookup. The composite preserves priority by the order of the `IEnumerable<IDocsLookup>` (local first, GitHub second) and implements union/deduping for listings and first-success semantics for reads; individual provider failures do not prevent other providers from being used.

## Example
```csharp
// In Program.cs or Startup.cs when configuring services
public void ConfigureServices(IServiceCollection services)
{
    // 'Configuration' is an IConfiguration instance (e.g. injected into Startup or available in Program)
    DependencyInjection.AddDocsLookup(services, Configuration);

    // other service registrations...
}
```

## Notes
- The order of the registered IDocsLookup instances determines priority: local docs are consulted before GitHub.
- Ensure configuration contains the expected sections for LocalDocsOptions and GitHubDocsOptions; missing GitHub token simply results in unauthenticated API calls (subject to rate limits).
- Two named HttpClients are registered for GitHub: the API client (used for listing) and the raw client (used for content reads); both use a 15s timeout and set a User-Agent header.


---

## AddInfrastructure

> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

Registers the application's infrastructure services into an IServiceCollection: configures EF Core (SQLite) DbContext, binds and configures project file storage options, and registers scoped repository, unit-of-work and file-service implementations. Use this extension in your application's startup (Program.cs or Startup) to compose all infrastructure dependencies in one call.

## Remarks
Centralizes dependency injection wiring for persistence and storage concerns so callers only need a single call to bring the infrastructure layer online. It obtains a connection string named "Default" from IConfiguration and falls back to an on-disk SQLite file (Data Source=gabriel.db) when none is provided. ProjectFilesOptions are bound from configuration (Projects:Files by default) and a disk-backed IProjectFileService is registered; the implementation persists files under the configured Root path using a per-project folder pattern (comment indicates {Root}/{ProjectId:N}). The method also delegates registration of chat provider, web search, web fetch and docs lookup components to helper methods.

## Example
```csharp
// In a minimal Program.cs or Startup.cs
var builder = WebApplication.CreateBuilder(args);
// builder.Configuration is the IConfiguration instance
builder.Services.AddInfrastructure(builder.Configuration);
var app = builder.Build();
```

## Notes
- The connection fallback ("Data Source=gabriel.db") creates/uses a SQLite file relative to the process working directory; provide a "Default" connection string to override this.
- Registered types (DbContext and repositories/unit-of-work) use scoped lifetimes, which are appropriate for web request scopes but should be considered if used outside typical request lifetimes.
- ProjectFilesOptions must be present in configuration if you rely on non-default file storage paths; otherwise the disk service will use whatever defaults are provided by the options type.

---

## AddWebFetch

> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

Registers a pre-configured HTTP client and a URL fetcher used by the web_fetch tool. Call this during dependency-injection setup (for example inside ConfigureServices) when you want a single, shared HttpClient configured with a sensible timeout and a normal browser User-Agent, plus an IUrlFetcher implementation that will be used application-wide.

## Remarks
This centralizes HTTP fetch configuration to avoid per-call client creation and to reduce the chance of being blocked by sites that reject non-browser User-Agents. The method registers a named HttpClient (named with HttpUrlFetcher.HttpClientName) with a 15‑second timeout and default request headers for User-Agent and Accept-Language, and it registers HttpUrlFetcher as the singleton IUrlFetcher. Redirects are allowed; SSRF protection is applied against the final destination via request hooks (see the SSRF guard implementation for details).

## Example
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // other service registrations
    AddWebFetch(services);
}
```

## Notes
- The HttpClient uses a 15 second timeout; adjust if your scenarios need longer or shorter waits.
- A browser-like User-Agent header is intentionally used because many major sites reject blank or script-like UAs.
- The registered HttpClient is created via the DI IHttpClientFactory, so the client lifecycle is managed and you should not create/dispose HttpClient instances per request.
- Redirects are permitted; any SSRF guard runs against the final redirected destination, not only the initially requested URL.
- IUrlFetcher is registered as a singleton; its implementation should be safe for concurrent use.

---

## AddWebSearch

> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

Registers and wires web-search providers based on configuration. Reads the Tools:Web:Active comma-separated list (defaulting to "ddg") and registers the corresponding provider HttpClients, options and concrete singleton implementations. When multiple providers are requested it composes them with a CompositeWebSearch (so results from different providers are merged and cross-provider hits rank first); with exactly one provider the composite is bypassed and that provider is registered directly as IWebSearch. This method is used at startup to enable one-or-many external web-search backends without changing application code.

## Remarks
AddWebSearch centralizes provider selection and the per-provider dependency wiring so the rest of the application depends only on IWebSearch. Each known provider key (e.g. "brave", "tavily", "ddg") maps to registration of a named HttpClient, configuration options (where applicable), and a concrete singleton search implementation. Per-call metrics are recorded by wrapping implementations with an InstrumentedWebSearch decorator that writes into the generic IMetricRecorder; metric reads are later exposed by the diagnostics endpoint via IMetricRepository. The method normalizes and de-duplicates requested keys and tolerates unknown keys by dropping them with a warning rather than failing the startup.

## Example
```csharp
// app startup: configure services
public void ConfigureServices(IServiceCollection services)
{
    // IConfiguration "config" is available here (e.g. from Startup constructor)
    AddWebSearch(services, Configuration);
    // other service registrations...
}

// appsettings.json (conceptual): choose providers
// "Tools:Web:Active": "tavily,brave,ddg"
```

## Notes
- The Tools:Web:Active value is parsed case-insensitively, trimmed, and duplicate entries are removed; unknown keys are ignored and logged as warnings.
- Brave and Tavily require corresponding configuration sections (BraveSearchOptions, TavilySearchOptions) and any API keys they need; missing credentials will cause runtime failures when those providers are used.
- When only one provider is active the CompositeWebSearch is not used and the single provider implementation is registered directly as IWebSearch; adding or removing providers requires an application restart because configuration is read at startup.

---

## ConfigureDdgHttpClient

> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

Registers a named HttpClient used by DuckDuckGoWebSearch with settings tuned for reliable search interactions: a 15 second request timeout, a primary HttpClientHandler that enables automatic decompression and a long-lived CookieContainer, and a handler lifetime of one hour. Use this registration when you need searches to share a session (cookies set by DDG's homepage) and when per-request headers (like rotating User-Agent and Referer) are applied by DuckDuckGoWebSearch rather than pinned on the client.

## Remarks
This helper centralizes the named-client configuration so multiple wiring paths (the active-providers path and the empty-config fallback) share identical behavior. The combination of a persistent CookieContainer and an extended handler lifetime preserves session cookies produced during the homepage pre-warm so subsequent /html/ and /lite/ requests appear as continuous sessions rather than cold, one-shot requests — which avoids triggering DDG's first-request anomaly heuristics. DefaultRequestHeaders are intentionally not set here because per-request header mutation (UA rotation, Sec-Fetch-Site, Referer) is performed at the HttpRequestMessage level in DuckDuckGoWebSearch.

## Notes
- Do not move User-Agent or other rotating headers into DefaultRequestHeaders on this client: doing so would pin a single UA for the handler lifetime and defeat rotation.
- AutomaticDecompression is set to All to ensure ReadAsStringAsync returns valid text; without it responses negotiated with gzip/deflate/br can appear as unreadable bytes.
- The handler lifetime is increased from the default (2 minutes) to one hour so the CookieContainer isn't replaced mid-conversation; be aware longer lifetimes can delay DNS updates and should be chosen to balance session persistence against DNS freshness.

---

## ConfigureGrokResilience

> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

Configures HttpStandardResilienceOptions for the Grok SSE/chat streaming scenario by applying a single total timeout to both the overall request and each attempt, and by lengthening the circuit-breaker sampling window. Reach for this helper when wiring the Grok provider's resilience pipeline so long-running streaming responses are not cancelled by the default, shorter per-attempt timeouts.

## Remarks
This method intentionally sets AttemptTimeout equal to the provided totalTimeout so that a single, generous timeout governs both the overall operation and individual attempts; the default split (short per-attempt timeout plus longer total) tends to prematurely terminate non-trivial streaming generations. Retries are left at their default behavior and only apply before a response stream starts — once tokens begin flowing the pipeline does not interrupt that attempt. The circuit-breaker SamplingDuration is expanded to twice the totalTimeout to satisfy the framework's SamplingDuration >= 2 * AttemptTimeout requirement and to reduce sensitivity over the longer streaming window.

## Example
```csharp
// During DI setup for the Grok provider, use a configured timeout value
var grokTimeout = TimeSpan.FromSeconds(configuration.GetValue<int>("Providers:Grok:TimeoutSeconds"));
ConfigureGrokResilience(opts, grokTimeout);
```

## Notes
- Setting AttemptTimeout to the same value as TotalRequestTimeout means there is no shorter per-attempt deadline; if you need shorter retries between attempts, adjust AttemptTimeout separately.
- Increasing SamplingDuration makes the circuit-breaker less reactive (it samples over a longer window), which is intentional for long-lived streams but may delay tripping on sustained failures.
- Ensure the supplied totalTimeout is large enough for expected streaming durations; otherwise long-running streams will still be cut off.

---