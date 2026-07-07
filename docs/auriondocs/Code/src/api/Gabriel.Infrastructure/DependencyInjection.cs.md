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

```csharp
public static class DependencyInjection
```


DependencyInjection_overview is a central static class that wires infrastructure services into the DI container. It orchestrates project-file storage registration, a shared HttpClient-based web fetcher, and a configurable web-search wiring system that can merge multiple providers or fall back to a default provider.

## Remarks

This abstraction encapsulates cross-cutting infrastructure concerns behind a single entry point, enabling the rest of the application to remain agnostic to concrete implementations. It coordinates provider wiring with instrumentation so per-provider failures and performance metrics can be observed via the InstrumentedWebSearch decorator and the diagnostics endpoints. Configuration-driven behavior is evident in how web-search providers are selected (Tools:Web:Active) and how the composite vs. single-provider path is chosen.

## Notes

- Unknown provider keys in Tools:Web:Active are dropped with a warning, so typos won’t crash startup or derail the web-search wiring.
- When configured for a single provider, the provider is wrapped with InstrumentedWebSearch to preserve metrics without incurring the composite merge overhead.
- A long-lived HttpClientHandler with a CookieContainer is used for the web fetch/provider clients; altering lifetimes or cookie handling can impact session persistence and response behavior.

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


AddChatProvider wires up chat providers into the DI container, always registering a MockChatProvider as a safe fallback and conditionally wiring a Grok-based provider when a Providers:Grok section exists in configuration with at least one model. Use this during startup to centralize provider wiring and configuration validation rather than sprinkling provider setup across the codebase.

## Remarks

Centralizes provider wiring and configuration: discovery of Grok settings, binding of options, and setup of authentication and HTTP clients are encapsulated behind a single entry point. The method enforces configuration requirements (ApiKey, BaseUrl, TimeoutSeconds, and valid model definitions) at startup, while keeping secrets handling isolated from codegen/migration paths. It uses environment-based gating (SKIP_DB_INIT) to avoid false failures in tooling scenarios, ensuring normal startup validation occurs only when secrets are available. If Grok is not configured, the method yields a safe default by leaving only the MockChatProvider registered.

## Notes

- At most one IsActive model is allowed; enabling multiple default models will fail validation.
- BaseUrl must be a valid absolute URL ending with '/'.
- During code generation or migrations, the code may skip certain validations if SKIP_DB_INIT is set; ensure this is intentional in those scenarios.


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


Configures the dependency injection container to wire up the model-facing documentation lookup system. It registers a LocalDocsLookup as the primary source and a GitHubDocsLookup as a fallback, then exposes a CompositeDocsLookup as IDocsLookup that queries local entries first and falls back to GitHub if needed. This method is intended to be invoked during application startup (e.g., in ConfigureServices) to ensure downstream components can resolve IDocsLookup-backed docs without knowing about the underlying sources. It also configures two HttpClients for the GitHub source (one for the API listing and one for raw content) and wires up the corresponding options.

## Remarks
By composing multiple lookups, this abstraction hides the complexity of where docs come from and provides graceful fallback. The order of resolution deliberately prioritizes the local, LLM-native docs to minimize network calls and ensure deterministic results; if the local docs are missing or outdated, the composite seamlessly consults GitHub. The pattern improves resilience and testability by isolating configuration of HttpClients and options.

## Example
```csharp
// Within startup DI configuration (in the same class that can access the private method)
AddDocsLookup(services, Configuration);
```

## Notes
- Ensure LocalDocsOptions and GitHubDocsOptions sections exist in the configuration so the options bind correctly.
- GitHub token is optional; if provided, the Authorization header is added to requests.
- The resolution order (Local first, then GitHub) and the named HttpClients influence which sources are queried and how requests are authenticated; altering these can change docs availability and performance.

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


Registers infrastructure services into the DI container, configuring the AppDbContext with SQLite using a Default connection string (falling back to a local gabriel.db) and registering repositories and a UnitOfWork as scoped services. It also wires up disk-backed project file storage via ProjectFilesOptions, and initializes several cross-cutting providers (Chat, WebSearch, WebFetch, DocsLookup) through their respective Add* calls.

## Remarks
Centralizes all infrastructure bootstrapping in one place, decoupling startup concerns from application logic. The AddInfrastructure extension ensures per-request scope for the DbContext-derived services and repositories, promoting transactional consistency within a request. It also reads configuration for the File storage options, enabling environment-specific paths without code changes. The modular AddChatProvider, AddWebSearch, AddWebFetch, and AddDocsLookup calls illustrate a pluggable infrastructure that can be extended or replaced without touching consuming components.

## Notes
- Fallback connection string uses a local SQLite file (gabriel.db); ensure the hosting environment permits file creation or provide a concrete connection string in configuration.
- Repositories and UnitOfWork are registered as scoped; their DbContext lifetime aligns with a single request, so avoid storing DbContext instances beyond the scope of a request.
- Disk-based project file storage persists under {Root}/{ProjectId:N}; ensure the root path is writable and that project IDs are available and correctly configured in ProjectFilesOptions.

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


It configures a named HttpClient for the web-fetching pipeline and wires a concrete URL fetcher into the dependency injection container. It applies browser-like defaults (a realistic User-Agent and Accept-Language) and a 15-second timeout, and it allows redirects so the final destination can be validated by the SSRF guard in request hooks.

## Remarks
By centralizing HttpClient configuration here, the code avoids scattering HTTP defaults across callers and enables reuse of the HttpClient through the IUrlFetcher abstraction. The named client (HttpClientName) isolates these settings from other HTTP usage and leverages HttpClientFactory’s lifetime management, reducing socket exhaustion and simplifying testing by keeping networking concerns behind IUrlFetcher.

## Notes
- The HttpClient is registered under a specific name (HttpUrlFetcher.HttpClientName); changes here affect only that named client.
- The 15-second timeout is per-request; long-fetch scenarios may require adjusting this value or introducing retry/backoff behavior.
- Redirects are allowed; ensure any downstream security or SSRF checks rely on the final destination as noted in the comments.

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


Configures and wires web search providers into the dependency injection container according to runtime configuration. It reads Tools:Web:Active from the configuration (defaulting to ddg) to determine which providers to enable, wires each selected provider with its HttpClient and options, and registers the concrete provider types as singletons. If only one provider is enabled, that provider is registered directly as IWebSearch; if multiple providers are enabled, their implementations are composed behind a CompositeWebSearch to merge and rank results. Per-provider usage is recorded via InstrumentedWebSearch, so diagnostics can query how each provider performed without changing provider implementations.

## Remarks
Architecturally, this function centralizes all provider wiring and instrumentation, enabling pluggable search backends without client code changes. It hides provider-specific HttpClient configuration behind a common interface and defers to CompositeWebSearch for result fusion. The InstrumentedWebSearch decorator isolates telemetry concerns from the providers themselves.

## Notes
- Unknown keys in Tools:Web:Active are ignored with a warning (non-fatal).
- If multiple providers are enabled, their results are merged and ranked by CompositeWebSearch; with a single provider, the concrete IWebSearch implementation is used directly.


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


Configures and registers a named HttpClient for the DuckDuckGoWebSearch integration, centralizing transport setup so the active-providers path and the empty-config fallback share a single source of truth.

It uses a long-lived HttpClientHandler with a dedicated CookieContainer to preserve cookies across requests, enabling session continuity during multi-turn interactions, and sets a 15-second timeout and automatic decompression.

Per-request headers are applied on each HttpRequestMessage; DefaultRequestHeaders here would pin a single User-Agent for the lifetime of the handler, which is avoided.

## Remarks

Decouples HTTP transport concerns from higher-level logic, enabling consistent session behavior and header rotation across the different configuration paths. It also preserves cookies across requests to mirror a real browser session, reducing the likelihood of DDG heuristics triggering on subsequent requests during a chat flow.

## Notes

- Do not rely on DefaultRequestHeaders to rotate User-Agent; per-request headers are set on each request to support UA rotation and contextual headers.
- CookieContainer persists cookies across requests for the duration of the handler; if per-user isolation is required, consider separate named HttpClients or resetting the cookie container appropriately.
- Endpoints in this scenario are absolute for different subdomains (html/ vs lite/); sharing a single BaseAddress would not be appropriate.

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


This method configures a standard resilience pipeline specifically tuned for Server-Sent Events (SSE) chat streams. It adjusts the default timeout settings, which are otherwise too short for non-trivial streaming operations, by setting both the total request timeout and per-attempt timeout based on a provided duration. Additionally, it configures the circuit breaker sampling duration to be twice the total timeout to comply with framework validation rules and ensure proper circuit breaker behavior.

## Remarks
The method addresses the challenge of maintaining long-lived streaming connections without premature termination by aligning timeouts with provider-specific settings. It also ensures that retries only occur before the response stream starts, allowing network or initial server errors to be retried while leaving ongoing token streams uninterrupted. The extended circuit breaker sampling duration helps avoid false positives in circuit breaking during these extended streaming attempts.

---