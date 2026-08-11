# InfisicalConfigurationProvider

> **File:** `src/api/Gabriel.API/Configuration/InfisicalConfigurationProvider.cs`  
> **Kind:** class

```csharp
public class InfisicalConfigurationProvider : ConfigurationProvider
```


InfisicalConfigurationProvider pulls secrets from a self-hosted Infisical instance at startup and merges them into IConfiguration. It authenticates using clientId and clientSecret, fetches environment-scoped secrets, and flattens the results by converting Infisical keys that use double-underscore separators into colon-delimited configuration keys, enabling IOptions binding (for example, Providers:Grok:ApiKey). This bootstrap provider runs during configuration building (before the DI container exists) and populates Data so configuration keys are available to the application startup.

## Remarks
InfisicalConfigurationProvider encapsulates remote secret loading as a bootstrap concern, isolating network I/O from the rest of the configuration system. The key transform (__ to :) aligns Infisical secret naming with .NET configuration paths, so secrets become first-class config entries usable by IOptions. Because Load runs during configuration building, failures are logged to Console.Error instead of crashing startup; missing keys surface only when accessed. HttpClient is created ad-hoc for the bootstrap and disposed afterward, avoiding IHttpClientFactory reliance at this early stage.

## Notes
- If authentication or secrets fetch fails, the exception is swallowed by Load and only writes to Console.Error; secrets may be unavailable until accessed.
- Keys are normalized by replacing __ with :, so ensure Infisical secret names follow this convention (e.g., PROVIDERS__GROK__APIKEY maps to Providers:Grok:ApiKey).
- Load uses a synchronous wait pattern (GetAwaiter().GetResult()) to bridge async startup code; in unusual synchronization contexts this could block, but it is intentional during configuration bootstrap.