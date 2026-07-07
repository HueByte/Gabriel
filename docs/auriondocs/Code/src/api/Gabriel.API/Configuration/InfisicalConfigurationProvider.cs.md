# InfisicalConfigurationProvider

> **File:** `src/api/Gabriel.API/Configuration/InfisicalConfigurationProvider.cs`  
> **Kind:** class

```csharp
public class InfisicalConfigurationProvider : ConfigurationProvider
```


InfisicalConfigurationProvider is a custom configuration source that pulls secrets from a self-hosted Infisical instance during application startup and merges them into IConfiguration. It authenticates against Infisical, fetches workspace/environment secrets, and exposes them as configuration keys by translating Infisical keys that use __ separators into colon-delimited configuration paths (for example, PROVIDERS__GROK__APIKEY becomes Providers:Grok:ApiKey). The provider runs in the bootstrap phase, before the DI container is available, so there is no ILogger yet; failures are written to Console.Error and do not crash startup, with missing keys evaluated later as usual when accessed. The retrieved secrets are stored in a case-insensitive dictionary and become available to IOptions binding and direct IConfiguration lookups.

## Remarks
InfisicalConfigurationProvider centralizes remote secret retrieval at startup, decoupling authentication and network access from the rest of the configuration surface. By translating Infisical’s key format into the familiar IConfiguration path syntax, it enables strongly-typed option binding (via IOptions) while preserving environment- and workspace-scoped secrets within the app's configuration graph.

## Notes
- Runs during configuration building (pre-DI); HttpClientFactory is not available, so a short-lived HttpClient is created and disposed in the bootstrap call.
- If authentication or secret fetch fails, the error is written to Console.Error and startup continues; however, any secret keys may be missing until accessed, at which point failures may surface as runtime errors.
- The mapping uses a case-insensitive key dictionary, enabling consistent binding regardless of key casing.