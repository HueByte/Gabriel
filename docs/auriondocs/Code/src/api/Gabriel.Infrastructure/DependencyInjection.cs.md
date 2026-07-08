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


The DependencyInjection class provides an AddInfrastructure extension on IServiceCollection to register infrastructure services, including disk-backed project file storage configured from Projects:Files (persisting under a {Root}/{ProjectId:N} path). It also outlines internal helpers for wiring a web page fetcher (a single HttpClient with sensible defaults and UA handling) and a web search pipeline that can combine multiple providers (with instrumentation and fallback behavior) as described in the comments. You’d reach for this during application startup when configuring the DI container to enable infrastructure features.

## Remarks
Architecturally, it serves as a centralized wiring hub for infrastructure concerns, outlining how web-search providers are composed and instrumented so telemetry and per-provider behavior remain consistent regardless of the provider mix.

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


Bootstraps chat providers by always registering MockChatProvider as the safe default and conditionally wiring Grok support when a Providers:Grok section with at least one active model exists. It uses the standard Options pipeline to bind and validate Grok configuration, reads secrets via the PROVIDERS__GROK__APIKEY environment key, and only wires Grok when configured, keeping MockChatProvider as the fallback to prevent crashes and to support UI fallback in development.

## Remarks
This method encapsulates provider wiring and startup validation, separating the concerns of a guaranteed fallback provider from the optional, config-driven Grok integration. It enables clean bootstrapping of real providers without impacting consumers, and it wires authentication and HTTP clients in a provider-safe, testable manner for Grok while preserving a stable default path.

## Notes
- Grok is registered only if a Providers:Grok section exists and contains at least one active model; otherwise only the MockChatProvider is registered.
- Startup validation runs for Grok options (ApiKey, BaseUrl, TimeoutSeconds, Models) unless the SKIP_DB_INIT environment flag is set to true; a misconfiguration will fail startup.
- Bearer authentication for Grok is applied through a DelegatingHandler (GrokAuthHandler) rather than DefaultRequestHeaders, enabling seamless key rotation without recreating HttpClient instances.
- HttpClient.Timeout is left effectively infinite to let the resilience pipeline govern request timeouts.

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


Configures the documentation lookup pipeline at startup by wiring together two sources into a single, prioritized provider. LocalDocsLookup serves as the primary on-disk, model-facing documentation source, while GitHubDocsLookup acts as a fallback that retrieves docs from GitHub. The method constructs a CompositeDocsLookup that queries LocalDocsLookup first and GitHubDocsLookup second, ensuring a resilient and deterministic lookup flow. It also wires two named HttpClient instances (one for the GitHub API and one for raw content), applies GitHub-related options from configuration, and registers the composed IDocsLookup as the application's implementation. Invoke this during service configuration to enable the repository of documentation entries the model and tooling rely on. 

## Remarks

This method centralizes the DI wiring for the documentation subsystem, insulating callers from the details of which sources exist or how they are composed. By using a CompositeDocsLookup with a defined priority (local first, then GitHub), failures in one source do not poison the others, and lookups transparently fall back to alternatives. The HTTP clients and options binding are encapsulated here, providing a single, configurable integration point for sourcing docs from either local assets or GitHub.

## Notes

- LocalDocsLookup is registered as a singleton and serves as the primary source; GitHubDocsLookup is registered as a singleton and serves as the fallback. The CompositeDocsLookup enforces the priority order when listing or reading docs.
- The GitHub HTTP client path is configured with a dedicated API client (for ListAsync) and a Raw client (for ReadAsync); the Authorization header is added only if a Token is supplied in GitHubDocsOptions.
- Configuration sections LocalDocsOptions.SectionName and GitHubDocsOptions.SectionName drive the wired options; ensure these sections exist and provide valid values for Owner, Repo, Branch, DocsPath, and related fields.
- The method is private, intended to be invoked as part of the application's startup bootstrap within its defining class; external callers cannot call it directly.

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


Bootstraps the application's infrastructure by wiring the data access layer, repositories, and ancillary providers in a single, cohesive startup path. AddInfrastructure configures SQLite-backed persistence (using the Default connection string if present, or a local gabriel.db file as a fallback), registers AppDbContext and a set of repositories, binds ProjectFilesOptions from configuration, and exposes a disk-backed implementation for project file storage. It also orchestrates the registration of chat, web search, web fetch, and docs lookup providers to keep startup concerns centralized and easily extensible.

## Remarks
This method centralizes infrastructure bootstrap, ensuring consistent lifetimes and configuration for the DbContext and repositories while isolating persistence concerns from the rest of the app. Binding ProjectFilesOptions and registering DiskProjectFileService make file storage configurable and swap-friendly, promoting testability and easier maintenance. By coordinating provider registrations (Chat, WebSearch, WebFetch, DocsLookup) here, startup remains cohesive and extensible.

## Notes
- If the configuration does not provide a Default connection string, a local SQLite database named gabriel.db is used as a fallback.
- Disk-backed project file storage relies on ProjectFilesOptions.Root being a valid, writable path; ensure the directory exists and the application has write permissions.
- AddInfrastructure registers DiskProjectFileService as IProjectFileService; replacing this implementation requires adjusting the DI registrations or supplying an alternative implementation.

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


Configures the dependency injection container to provide a web URL fetcher by registering a named HttpClient with sensible defaults and wiring HttpUrlFetcher as the IUrlFetcher implementation. This centralizes HTTP fetch configuration so external resources can be retrieved through a single, testable service.

## Remarks
Centralizes HTTP fetch concerns behind IUrlFetcher to decouple callers from HttpClient details. The named HttpClient pattern keeps its configuration scoped to this fetch path, enabling different outbound HTTP strategies elsewhere. Registering HttpUrlFetcher as a singleton means the fetcher should be thread-safe and stateless across calls.

## Notes
- The User-Agent and Accept-Language headers are hard-coded to improve compatibility with major sites; changing them can affect responses.
- HttpClientFactory manages lifetimes; avoid disposing the named client directly.
- If you need different settings for other external endpoints, define a separate named client rather than reusing HttpUrlFetcher.HttpClientName.

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


Configures and wires the web-search providers into the DI container based on Tools:Web:Active, registering each provider's HttpClient and concrete implementation, and wrapping the final set in InstrumentedWebSearch for metrics reporting. If Tools:Web:Active is not provided, it defaults to DuckDuckGo; unknown provider keys are ignored with a warning.

## Remarks
AddWebSearch centralizes provider wiring and enables runtime extensibility by selecting providers from configuration rather than hard-coding them. The InstrumentedWebSearch decorator applies cross-cutting metrics to all provider calls without requiring each provider to implement its own instrumentation. The approach simplifies testing and extension: to add a new provider, extend the switch with its own registration path and options class, leaving the rest of the wiring intact.

## Notes
- If a provider key appears that is not recognized, it is dropped with a warning rather than causing a failure.

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


Configures and registers the named HttpClient for DuckDuckGoWebSearch and centralizes its shared HTTP behavior so the active-providers path and the empty-config fallback share one source of truth. It wires a long-lived HttpClientHandler with a CookieContainer and automatic decompression to preserve session state across requests, while per-request headers such as User-Agent rotation are applied at request time by DuckDuckGoWebSearch rather than being pinned to the client lifetime.

## Remarks
By centralizing this configuration, any adjustments to timeout, decompression, cookies, or handler lifetime propagate to all users of the named client, keeping behavior consistent across code paths. The CookieContainer ensures session continuity across homepage and subsequent /html/ or /lite/ requests, aligning with how real browsers maintain a session.

## Notes
- Do not use DefaultRequestHeaders here; per-request headers are set on each HttpRequestMessage to rotate User-Agent and related context.
- SetHandlerLifetime(TimeSpan.FromHours(1)) means the HttpMessageHandler is reused for up to an hour; consider alignment with DNS/Cookie TTL if your deployment changes endpoints frequently.

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


Configures the resilience policy for the Grok streaming path used by the SSE chat pipeline. It applies the provided totalTimeout to both TotalRequestTimeout and AttemptTimeout, ensuring the overall request and each individual attempt share a single timeout window and preventing long-running streaming generations from being terminated mid-stream by a too-aggressive default. It also widens CircuitBreaker.SamplingDuration to twice the total timeout to satisfy the framework's requirement that sampling spans at least two attempts. This total timeout is typically sourced from Providers:Grok:TimeoutSeconds and applied consistently across the pipeline.

## Remarks
Centralizes resilience tuning for streaming workloads; callers adjust a single total timeout and have it reflected across the per-attempt and circuit-breaker settings. The approach ensures retries occur before the response stream starts, protecting against transient DNS/network issues, while the widened circuit-breaker window provides guards during longer streaming segments.

## Notes
- Mutates the options instance in place; call this during startup before handling requests.
- Keep totalTimeout in sync with Providers:Grok:TimeoutSeconds to avoid inconsistent behavior.

---