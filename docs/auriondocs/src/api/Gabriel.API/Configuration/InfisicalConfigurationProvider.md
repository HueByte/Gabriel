# InfisicalConfigurationProvider

> **File:** `src/api/Gabriel.API/Configuration/InfisicalConfigurationProvider.cs`  
> **Kind:** class

Pulls secrets from a self-hosted Infisical instance at startup and merges them into the IConfiguration data store so values can be consumed via IConfiguration/IOptions. Use this provider when you want Infisical-managed secrets available as configuration keys during application bootstrap (the provider runs during configuration build-out, before the DI container is available).

## Remarks
This provider runs as part of IConfiguration construction and is intentionally self-contained: it uses a short-lived HttpClient (because IHttpClientFactory/DI are not available at this stage), authenticates to Infisical, fetches raw secrets, and writes them into the provider's Data dictionary. Secret keys returned by Infisical that contain "__" are translated to ":" so they follow the environment-variable-to-configuration convention (e.g., PROVIDERS__GROK__APIKEY -> Providers:Grok:ApiKey). Failures during the load are written to standard error and do not stop application startup; that makes the load best-effort and defers missing-secret failures to later when code actually reads the configuration.

## Example
```csharp
// If Infisical returns a secret with key:
// "PROVIDERS__GROK__APIKEY" = "secret-value"
// the provider will expose it in IConfiguration as:
// Providers:Grok:ApiKey => "secret-value"

// Resolving via IConfiguration or IOptions<T> works as usual:
var apiKey = configuration["Providers:Grok:ApiKey"]; // "secret-value"
```

## Notes
- The provider executes synchronously during configuration build (Load blocks on async work); network delays will delay startup. Use the configured timeout to limit wait time.
- Because it runs before DI is available it creates and disposes an HttpClient directly; this is acceptable here since the load runs once at bootstrap.
- Errors during authentication or fetch are caught and written to Console.Error (with long response bodies truncated) so startup continues — missing or invalid secrets will surface later when the application attempts to read them.