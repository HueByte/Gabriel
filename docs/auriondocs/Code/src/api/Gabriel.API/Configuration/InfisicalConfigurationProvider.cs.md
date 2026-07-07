# InfisicalConfigurationProvider

> **File:** `src/api/Gabriel.API/Configuration/InfisicalConfigurationProvider.cs`  
> **Kind:** class

```csharp
public class InfisicalConfigurationProvider : ConfigurationProvider
```


InfisicalConfigurationProvider pulls secrets from a self-hosted Infisical instance at startup and merges them into IConfiguration. Secret keys that contain double underscores (__) are mapped to colon-delimited configuration paths, so keys like PROVIDERS__GROK__APIKEY become Providers:Grok:ApiKey for IOptions binding.

## Remarks
Why this abstraction exists: to bootstrap application configuration with Infisical secrets during startup when the DI container hasn't been wired yet. It performs a minimal, one-shot HTTP bootstrap using a short-lived HttpClient and stores the results into the IConfiguration Data store so they participate in binding. If loading fails, the error is written to standard error to avoid crashing startup; missing keys will fail loudly when accessed later.

## Notes
- The provider runs during configuration building (pre-DI). If the load fails, startup continues but the secret keys won't be available until accessed.
- Secret keys are transformed by replacing "__" with ":" and stored in a case-insensitive dictionary, producing hierarchical config paths like "Providers:Grok:ApiKey".
- The implementation uses a small HttpClient with a BaseAddress derived from Host and a configurable Timeout; network issues or invalid credentials surface as InvalidOperationException with a truncated response body.