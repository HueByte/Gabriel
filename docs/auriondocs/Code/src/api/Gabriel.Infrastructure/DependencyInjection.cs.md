# DependencyInjection.cs

> **Source:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`

## Contents

- [DependencyInjection](#dependencyinjection)
- [AddChatProvider](#addchatprovider)
- [AddDocsLookup](#adddocslookup)
- [AddInfrastructure](#addinfrastructure)
- [AddWebFetch](#addwebfetch)
- [AddWebSearch](#addwebsearch)
- [ConfigureDdgHttpClient](#configureddghttpclient)
- [ConfigureGrokResilience](#configuregrokresilience)

---

## DependencyInjection
> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** class

```csharp
public static class DependencyInjection
```


Centralizes the bootstrapping of the infrastructure layer. Call AddInfrastructure on your IServiceCollection to register project-file storage, a configured web-fetch HttpClient, and the multi-provider web-search wiring (including instrumentation, fallbacks, and per-provider lifecycle considerations) so the application can access persistent storage, fetch web content, and perform searches through multiple providers.

## Remarks
This abstraction isolates infrastructure concerns from business logic, enabling environment-specific wiring without touching core application code. It enables a pluggable WebSearch strategy: multiple providers can be registered and wrapped with a metrics/decorator to surface per-provider health while preserving a single, unified search path; a safe fallback prevents total failure if configuration is incomplete or invalid. The AddInfrastructure method coordinates internal builders (AddWebFetch, AddWebSearch) so future providers or wiring strategies can be introduced with minimal impact to consumer code.

## Notes
- If Tools:Web:Active is missing or contains unknown keys, the wiring falls back to DuckDuckGo to keep search functionality available; verify config if you rely on specific providers.
- Do not create HttpClient instances manually; rely on IHttpClientFactory so the long-lived handler and cookie state described in the comments are preserved.
- The web-fetch HttpClient wiring accounts for UA string handling and redirect behavior to avoid blocks from major sites and to support the SSRF guard logic described in the comments.

---

## AddChatProvider
> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

```csharp
private static void AddChatProvider(IServiceCollection services, IConfiguration config)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `services` | `IServiceCollection` | — |
| `config` | `IConfiguration` | — |

**Returns:** `void`


Registers chat providers into the DI container. It always registers a default MockChatProvider to guarantee a provider is available and to support UI picker fallbacks; it conditionally wires the Grok provider when a Grok section exists in configuration and contains at least one model. When Grok is enabled, the method binds the GrokOptions from configuration and applies a battery of validations on startup: ApiKey must be present, BaseUrl must be a valid absolute URL, TimeoutSeconds must be positive, at most one active model is allowed, and each model entry must have a non empty Name and a positive ContextWindowTokens. If the environment variable SKIP_DB_INIT is not set to true, these validations are executed at startup. The code also reads a startup timeout from the Grok section, registers a GrokAuthHandler as a transient service, and registers a named HttpClient for GrokChatProvider so per request authorization is applied by the handler rather than DefaultRequestHeaders. This design centralizes provider discovery and keeps a safe fallback path for UI components.

---

## AddDocsLookup
> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

```csharp
private static void AddDocsLookup(IServiceCollection services, IConfiguration config)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `services` | `IServiceCollection` | — |
| `config` | `IConfiguration` | — |

**Returns:** `void`


Sets up the documentation lookup pipeline by wiring LocalDocsLookup and GitHubDocsLookup behind a CompositeDocsLookup with local-first priority. This method configures their options, HTTP clients, and DI registrations so a consumer querying IDocsLookup will first consult local docs and fall back to GitHub when needed.

## Remarks
Why this wiring exists is to decouple the source of truth for docs from the consumption site. The composite pattern encapsulates multiple sources so callers always use a single, stable interface (IDocsLookup) while the underlying sources can evolve independently. LocalDocsLookup acts as the primary source for performance and offline availability, with GitHubDocsLookup serving as a robust fallback.

## Notes
- This is a private helper invoked during startup; external code should not call it directly.
- GitHub token is optional; omitting it uses unauthenticated access (subject to GitHub API rate limits).

---

## AddInfrastructure
> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

```csharp
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `services` | `IServiceCollection` | — |
| `config` | `IConfiguration` | — |

**Returns:** `IServiceCollection`


Configures and wires the app's infrastructure services into the DI container. It creates and configures the SQLite-backed AppDbContext (using the Default connection string from configuration, with a local fallback), registers repositories and a unit of work, binds options for disk-based project file storage, and wires up the chat, web search, fetch, and docs lookup providers. Call this extension method during startup to centralize infrastructure bootstrap and maintain consistent lifetimes and configurations across the infrastructure layer.

## Remarks

By centralizing infrastructure registrations, AddInfrastructure provides a single bootstrap point for persistence, repositories, and external providers. It relies on IConfiguration to adapt to different environments without code changes, and binds ProjectFilesOptions to configure disk storage for project files (root path, size limits, allowed extensions, and content type prefixes). The chained AddChatProvider, AddWebSearch, AddWebFetch, and AddDocsLookup calls illustrate how infrastructure concerns are extended with optional capabilities, all wired from this entry point.

## Notes
- The SQLite connection string falls back to Data Source=gabriel.db when a named connection string is not found in configuration. Ensure this fallback is appropriate for your environment.
- ProjectFilesOptions.SectionName must point to a valid configuration section; otherwise the DiskProjectFileService will receive default values.
- Calling AddInfrastructure more than once can register duplicate services; invoke it once during application startup.

---

## AddWebFetch
> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

```csharp
private static void AddWebFetch(IServiceCollection services)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `services` | `IServiceCollection` | — |

**Returns:** `void`


Configures Dependency Injection to provide a browser-like HTTP fetcher by registering a named HttpClient and wiring HttpUrlFetcher as the IUrlFetcher implementation. It applies a 15-second timeout and browser-style headers (a Chrome-like User-Agent and Accept-Language) to improve compatibility with real web pages, and it allows redirects while the SSRF guard validates the final destination via request hooks.

## Remarks
Centralizes outbound web fetch configuration, ensuring consistent behavior across all components that depend on IUrlFetcher. By binding IUrlFetcher to a concrete HttpUrlFetcher and sharing the same HttpClient instance via IHttpClientFactory, the app gains testability and a single point of change for fetch-related behavior. The redirects are allowed because the SSRF guard inspects the final destination after redirection, balancing practicality with safety.

## Notes
- Ensure the HttpUrlFetcher.HttpClientName constant matches the name used here; a mismatch prevents DI from resolving the client.
- This method is private; its usage is confined to the DI bootstrap. If you need to customize it from outside, consider refactoring into a public extension.

---

## AddWebSearch
> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

```csharp
private static void AddWebSearch(IServiceCollection services, IConfiguration config)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `services` | `IServiceCollection` | — |
| `config` | `IConfiguration` | — |

**Returns:** `void`


Configures and wires the web search providers at application startup. It reads the Tools:Web:Active setting (defaulting to 'ddg' if missing), registers the HTTP clients and concrete search services for each known provider (Brave, Tavily, and DuckDuckGo), and returns the concrete types for decoration. The final IWebSearch is wrapped by InstrumentedWebSearch to enable metrics collection, and the set of registered providers is exposed to the composite search when multiple sources are enabled.

## Remarks
This method centralizes provider wiring behind a config-driven switch, so enabling or disabling providers is as simple as editing configuration rather than code. Each provider gets its own HttpClient configuration (base URL, timeout, and necessary headers) and a dedicated implementation registered as a singleton, allowing clean separation of concerns and straightforward testing.
Instrumentation is applied via InstrumentedWebSearch, so every search action is recorded by IMetricRecorder without changing the caller code. Downstream composition with CompositeWebSearch enables merging or ranking results across multiple enabled providers.

## Notes
- Unknown provider keys are ignored with a warning (the code path drops unknown keys rather than crashing), so typos in Tools:Web:Active are tolerated but should be corrected for predictability.
- If Tools:Web:Active is not supplied, the method defaults to registering DuckDuckGo (ddg) automatically.
- When enabling Brave or Tavily, ensure their respective configuration sections BraveSearchOptions and TavilySearchOptions exist and are wired, since the method reads SectionName, BaseUrl, ApiKey, and TimeoutSeconds to configure HttpClient behavior.

---

## ConfigureDdgHttpClient
> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

```csharp
private static void ConfigureDdgHttpClient(IServiceCollection services)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `services` | `IServiceCollection` | — |

**Returns:** `void`


Registers the named HttpClient used by DuckDuckGoWebSearch and centralizes its configuration so both the active-providers path and the empty-config fallback share a single source of truth. The registered HttpClient uses a long-lived HttpClientHandler with a CookieContainer to persist session cookies across requests, and it applies a 15-second timeout. Automatic decompression is enabled to correctly decode compressed responses, and the handler lifetime is set to one hour to avoid losing session state during multi-turn interactions. Per-request headers (such as User-Agent rotation) are applied by DuckDuckGoWebSearch on each HttpRequestMessage; setting DefaultRequestHeaders here would pin a single UA for the lifetime of the client and break UA rotation.

## Remarks
This abstraction groups HttpClient configuration in one place to ensure consistency across DI paths and to preserve session continuity with DuckDuckGoWebSearch by maintaining cookies and a refreshed handler lifetime. It helps minimize first-request anomalies when transitioning from homepage pre-warm to subsequent searches. Note that per-request headers are responsible for UA rotation, so the client itself does not fix a single User-Agent for its entire lifetime.

## Notes
- Do not pin a User-Agent or other default headers in this method; UA rotation is implemented per request by DuckDuckGoWebSearch.
- The HttpClient is registered under a specific named client; ensure the same name is used when injecting or consuming the client.
- Tuning the handler lifetime or cookie strategy can affect session persistence and DNS freshness; coordinate such changes with the surrounding session management logic.

---

## ConfigureGrokResilience
> **File:** `src/api/Gabriel.Infrastructure/DependencyInjection.cs`  
> **Kind:** method

```csharp
private static void ConfigureGrokResilience(HttpStandardResilienceOptions opts, TimeSpan totalTimeout)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `opts` | `HttpStandardResilienceOptions` | — |
| `totalTimeout` | `TimeSpan` | — |

**Returns:** `void`


Configures the resilience policy for the SSE Grok streaming path by aligning the overall request timeout, per-attempt timeout, and circuit-breaker sampling with a single total duration. This helper is intended for internal dependency-injection wiring and ensures long-running streaming generations aren’t terminated mid-stream by default timeouts, while still allowing pre-stream retries. Specifically, it applies the provided totalTimeout to both TotalRequestTimeout and AttemptTimeout, and sets the CircuitBreaker.SamplingDuration to twice the total timeout to satisfy the framework’s rule that SamplingDuration >= 2 * AttemptTimeout.

## Remarks
Centralizes streaming-focused resilience tuning so streaming scenarios behave predictably under long-lived token streams, reducing the risk of mid-stream termination. By mutating the supplied HttpStandardResilienceOptions to a streaming-friendly profile, it ensures consistency across callers during DI setup and avoids ad-hoc timeout configurations scattered throughout the codebase.

## Notes
- Mutates the provided HttpStandardResilienceOptions instance; ensure you pass the correct options object from the DI container.


---