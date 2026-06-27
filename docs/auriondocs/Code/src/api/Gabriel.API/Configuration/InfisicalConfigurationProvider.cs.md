# InfisicalConfigurationProvider

> **File:** `src/api/Gabriel.API/Configuration/InfisicalConfigurationProvider.cs`  
> **Kind:** class

Pulls secrets from a self-hosted Infisical instance at application startup and injects them into the IConfiguration key/value collection so they participate in normal configuration and IOptions binding. Use this provider when you want secrets stored in Infisical to be available via IConfiguration (including hierarchical config using ":" paths) during app bootstrap.

## Remarks
This provider is designed to run during configuration build-out (before the DI container is available). It authenticates to Infisical, fetches raw secrets for the configured workspace/environment/path, and loads them into the provider's Data dictionary. Secret names using the double-underscore convention (e.g. PROVIDERS__GROK__APIKEY) are translated to configuration paths (Providers:Grok:ApiKey) so existing IConfiguration and IOptions consumers can bind to them naturally. The implementation creates a short-lived HttpClient for the one-time bootstrap call and writes any bootstrap errors to stderr instead of throwing, allowing application startup to continue even if secret retrieval fails.

## Example
```csharp
// Example showing how Infisical-stored secret names map to IConfiguration keys:
// Infisical secret key:  PROVIDERS__GROK__APIKEY
// IConfiguration key:     "Providers:Grok:ApiKey"

string configKey = "Providers:Grok:ApiKey";
string? apiKey = configuration[configKey]; // reads the value populated by the provider
```

## Notes
- The provider runs before the DI container exists, so it uses a locally created HttpClient (not IHttpClientFactory).
- On any exception during Load the code writes an error to stderr and does not rethrow; if loading fails no secrets are added and consumers will see missing/null values.
- Keys are stored using StringComparer.OrdinalIgnoreCase, so lookups are case-insensitive.
- The synchronous Load override blocks on asynchronous calls via GetAwaiter().GetResult(); in environments with a synchronization context this pattern can deadlock, but it is safe in typical ASP.NET Core configuration bootstrap where Load runs during builder configuration.
- Response bodies included in error messages are truncated to avoid emitting large payloads (200 characters).