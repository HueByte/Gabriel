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

Registers and configures the infrastructure services used by the application — project file storage, web-fetching, and web-search providers — into an IServiceCollection. Call this from application startup (e.g., in Program.cs or the host builder) to ensure disk-backed project file storage is configured from Projects:Files and that the web tools (fetch and search) are registered with sensible HttpClient settings and metric instrumentation.

## Remarks
This wiring centralizes cross-cutting infrastructure concerns so the rest of the codebase can depend on high-level abstractions (IWebSearch, web-fetch tool, file storage) rather than concrete clients or handler settings. Web-search registration reads Tools:Web:Active (a comma-separated list of provider keys) and maps each recognized key to a provider-specific registration (named HttpClient + concrete implementation). Each provider is wrapped with a metrics decorator (InstrumentedWebSearch) so per-provider call outcomes are recorded by the generic IMetricRecorder; when multiple providers are enabled a CompositeWebSearch merges results and preserves per-provider error handling.

## Example
```csharp
// In Program.cs or the DI composition root
var builder = WebApplication.CreateBuilder(args);
// builder.Configuration is passed through
builder.Services.AddInfrastructure(builder.Configuration);
```

## Notes
- Unknown or misspelled provider keys in Tools:Web:Active are skipped with a warning; if no valid providers are found a DuckDuckGo provider is registered as a fallback so the tool remains functional.
- DuckDuckGo registration uses a long-lived CookieContainer and extends handler lifetime (approximately 1 hour) to keep session cookies across requests; per-request headers (User-Agent rotation, Sec-Fetch-Site, Referer) are applied in DuckDuckGoWebSearch itself, not via DefaultRequestHeaders.
- The web-fetch HttpClient is configured with a browser-like User-Agent and a non-trivial timeout because some sites reject blank or scriptable-looking UAs; redirects are allowed and an SSRF guard validates the final destination via request hooks.

---

## AddChatProvider

> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

Registers available chat providers into the IServiceCollection using configuration to decide which concrete providers to enable. Always provides a MockChatProvider singleton as a safe fallback; when a Grok provider section exists and contains at least one model, it binds and validates GrokOptions, registers the Grok auth handler, and configures a named HttpClient and resilience settings used by the GrokChatProvider.

## Remarks
This method centralizes DI wiring for chat providers so the application always has a usable provider (the mock) while allowing environment-driven enabling of real providers. Provider configuration is bound via the standard Options pipeline and validated early; the method intentionally reads certain runtime values (like TimeoutSeconds) once at startup to capture a stable setting for the HTTP/resilience pipeline. ValidateOnStart is skipped when SKIP_DB_INIT=="true" to avoid failures in environments (like codegen) where secrets are not available.

## Example
```csharp
// Called from host/Startup configuration to register chat providers
public void ConfigureServices(IServiceCollection services)
{
    AddChatProvider(services, Configuration);
}
```

## Notes
- ValidateOnStart will run option validators at host build; set SKIP_DB_INIT=true to skip this step in environments that lack secrets (e.g., code generation). 
- The Grok provider requires an ApiKey; the code expects it to be provided via the provider's named section (commonly as an env var like PROVIDERS__GROK__APIKEY).
- TimeoutSeconds is read once at startup for the provider's resilience pipeline; changing the config at runtime will not update that captured value.

---

## AddDocsLookup

> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

Registers and configures the documentation lookup stack into an IServiceCollection so the application can resolve an IDocsLookup that first consults a local LLM-native docs folder and falls back to GitHub-hosted documentation. Use during application startup (DI configuration) to expose a CompositeDocsLookup composed of LocalDocsLookup (primary) and GitHubDocsLookup (fallback).

## Remarks
This helper wires up three pieces: option bindings for LocalDocsOptions and GitHubDocsOptions, two named HttpClient entries used by the GitHub docs provider (one for the GitHub JSON API and one for raw content), and the lookup implementations themselves. The CompositeDocsLookup is registered as the IDocsLookup and is intentionally ordered so local docs take precedence and GitHub serves as a safe fallback; failures in one source do not poison the other. The ApiHttpClient is configured with standard GitHub headers and will include an Authorization header if a token is present in configuration.

## Example
```csharp
// typical use from a Startup/Program DI registration method
public void ConfigureServices(IServiceCollection services)
{
    // 'Configuration' is an IConfiguration instance
    AddDocsLookup(services, Configuration);
    // other service registrations...
}
```

## Notes
- Ensure the configuration sections for LocalDocsOptions and GitHubDocsOptions are present and populated (path for local docs, optional GitHub token/repo settings).
- The GitHub Api and Raw HttpClients use a 15-second timeout and specific named headers; supply a token to avoid GitHub rate limits if needed.
- The order of providers passed into CompositeDocsLookup determines priority (local first, then GitHub). The composite performs union/deduping for lists and tries sources in order for reads.
- Lookups and HttpClients are registered as singletons; ensure any stateful dependencies are safe to use as singletons.

---

## AddInfrastructure

> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

Registers the infrastructure-layer services (EF DbContext, repositories, file storage, and related integration services) into an IServiceCollection. Use this extension from application startup to compose the app's data and project-file services rather than registering each implementation individually.

## Remarks
This method centralizes wiring of the infrastructure dependencies: it configures the AppDbContext to use SQLite, registers scoped repository and unit-of-work implementations, binds ProjectFilesOptions from configuration, and registers a disk-backed project file service. It also delegates additional feature registrations (chat provider, web search, web fetch, and docs lookup) to helper methods so the application's Program/Startup only needs a single call to enable the full infrastructure surface.

## Example
```csharp
// Program.cs (minimal hosting)
var builder = WebApplication.CreateBuilder(args);

// register infrastructure using IConfiguration
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
// ...
```

## Notes
- If no connection string named "Default" is present in configuration, the method falls back to "Data Source=gabriel.db" and configures EF Core to use SQLite via UseSqlite.
- ProjectFilesOptions are bound from the configuration section identified by ProjectFilesOptions.SectionName (comment indicates "Projects:Files"), and the registered IProjectFileService is a disk-backed implementation that persists files under {Root}/{ProjectId:N}.
- Services registered here (DbContext, repositories, unit-of-work, project file service) use scoped lifetimes. Re-calling this extension will re-register the same services (last registration wins), so call it once during startup unless intentionally overriding registrations.


---

## AddWebFetch

> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

Registers a preconfigured HTTP fetch stack on the provided IServiceCollection. It adds a named HttpClient with a 15-second timeout and browser-like default request headers (User-Agent and Accept-Language) so requests look like a normal browser, and registers IUrlFetcher implemented by HttpUrlFetcher as a singleton. Call this from your application's service registration (e.g. ConfigureServices) when you need a robust, consistent way to fetch web pages.

## Remarks
This method centralizes web-fetch configuration to ensure consistent behavior across the app: using IHttpClientFactory avoids socket-exhaustion issues and lets callers request a named HttpClient with a sensible timeout. The browser-style User-Agent is deliberate because many sites reject requests that look like scripts or have no UA. Redirects are allowed; the codebase runs any SSRF checks against the final destination via request hooks, so the final redirected URL is what the SSRF guard evaluates.

## Example
```csharp
// inside Startup.ConfigureServices or equivalent
public void ConfigureServices(IServiceCollection services)
{
    // other registrations...
    AddWebFetch(services);
}
```

## Notes
- DefaultRequestHeaders.Add will throw if the same header is added more than once; ensure this registration runs only once or guard against duplicate header additions.
- The configured headers and timeout apply to every request made with the named client; change them here if you need different defaults for other callers.
- Timeout is set to 15 seconds; adjust if your target endpoints regularly require more time to respond.


---

## AddWebSearch

> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

Registers web-search provider implementations and their HttpClients into the application's IServiceCollection based on the Tools:Web:Active configuration setting. Call this during startup dependency injection wiring to enable DuckDuckGo, Brave or Tavily search backends (or a composite of multiple providers). The method binds provider-specific options, adds named HttpClients, registers concrete provider singletons, and then exposes an IWebSearch: if multiple providers are requested a CompositeWebSearch is used (merged, rank-aware); if only one provider is active that implementation is registered directly. All registered IWebSearch instances are decorated by InstrumentedWebSearch so calls are recorded to the generic metric infrastructure.

## Remarks
This centralizes all web-search plumbing (options binding, named HttpClient setup, provider singleton registration and the selection between a single-provider vs. composite implementation). It ensures per-provider HTTP configuration is applied consistently and that every provider call is recorded via InstrumentedWebSearch for diagnostics. Unknown provider keys in the Tools:Web:Active list are ignored with a warning, and the order of keys in the list does not affect result ranking because the merging logic is already rank-aware.

## Notes
- Accepted provider keys include variants like "ddg" or "duckduckgo", "brave" and "tavily"; typos are dropped with a warning and do not crash startup.
- Brave and Tavily require their respective options (API keys/base URLs/timeouts) to be present in configuration; missing or invalid settings will surface when the named HttpClient is configured or used.
- InstrumentedWebSearch depends on IMetricRecorder/IMetricRepository being registered (the codebase expects those to be provided by the Engine + EF wiring). If those services are not registered, metric recording or diagnostics may fail.

---

## ConfigureDdgHttpClient

> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

Registers a named HttpClient configured specifically for DuckDuckGoWebSearch and wired into the DI container. Use this when you need the client to behave like a real browser session for DuckDuckGo (persisting session cookies, handling compressed responses) while keeping per-request headers (User-Agent rotation, Referer, Sec-Fetch context) mutable.

## Remarks
This method centralizes the HTTP client configuration that both the active-providers path and the empty-config fallback share. It configures a short request timeout (15s) and a primary HttpClientHandler that enables automatic decompression and a long-lived CookieContainer so the session cookies set by DuckDuckGo's homepage persist into subsequent /html/ and /lite/ requests. The handler lifetime is increased to one hour to avoid the default 2-minute churn which would discard the cookie jar mid-conversation; BaseAddress is intentionally omitted because DuckDuckGo uses different subdomains and absolute URLs.

## Example
```csharp
// Typical usage inside ConfigureServices / Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // other registrations...
    ConfigureDdgHttpClient(services);
}

// DuckDuckGoWebSearch implementation should obtain the named client:
// var client = httpClientFactory.CreateClient(DuckDuckGoWebSearch.HttpClientName);
```

## Notes
- Do not set DefaultRequestHeaders on this named client: per-request headers (User-Agent rotation, Referer, Sec-Fetch-Site) must be applied on each HttpRequestMessage to preserve header rotation behavior.
- CookieContainer is shared for the lifetime of the handler: it preserves session state across requests but also means cookies are shared for all code using this named client while the handler lives — consider tenant/user isolation if that matters.
- Handler lifetime is set to one hour as a tradeoff: it prevents frequent cookie resets but can increase the chance of stale DNS entries if kept too long.

---

## ConfigureGrokResilience

> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

Configures the standard resilience settings used for Grok's server-sent-event (SSE) chat streams. It aligns the total request timeout and per-attempt timeout to the same value (driven by the provider's configured timeout) and increases the circuit-breaker sampling window so long-running streaming attempts are not cut off prematurely.

## Remarks
This method exists to avoid prematurely terminating long-lived SSE/chat responses: by making the attempt timeout equal to the overall request timeout the pipeline will not interrupt a streaming attempt once tokens start flowing. Retries are left to their default behaviour (they only run before the response stream starts), while the circuit-breaker sampling duration is widened to satisfy the framework validation rule requiring SamplingDuration >= 2 * AttemptTimeout and to reduce the chance of spurious trips during extended streams.

## Example
```csharp
// Use the configured provider timeout (e.g. Providers:Grok:TimeoutSeconds) to set resilience.
var grokTimeout = TimeSpan.FromSeconds(configuration.GetValue<int>("Providers:Grok:TimeoutSeconds"));
ConfigureGrokResilience(opts, grokTimeout);
```

## Notes
- This method mutates the supplied HttpStandardResilienceOptions instance in-place.
- Setting AttemptTimeout equal to the total request timeout intentionally removes a shorter per-attempt cutoff; do this only when streaming responses are expected.
- Retries occur only before the response stream begins, so network/connection failures benefit but in-stream errors are not retried by the pipeline.
- Doubling the TimeSpan ticks for the circuit-breaker sampling window could overflow for extreme TimeSpan values; use realistic timeout values (seconds to minutes) to avoid that edge case.


---