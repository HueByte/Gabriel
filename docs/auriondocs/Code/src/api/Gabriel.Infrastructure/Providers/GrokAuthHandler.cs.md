# GrokAuthHandler

> **File:** `src/api/Gabriel.Infrastructure/Providers/GrokAuthHandler.cs`  
> **Kind:** class

```csharp
internal sealed class GrokAuthHandler : DelegatingHandler
```


GrokAuthHandler is a DelegatingHandler that attaches the Grok API key as a Bearer token to every outbound request sent through the Grok-enabled HttpClient pipeline. It reads the key via `IOptionsMonitor<GrokOptions>`, so a rotated key can be picked up at runtime without recycling HttpClient or its handlers. This centralizes authentication for Grok API calls and avoids scattering token management across individual request sites.

## Remarks
GrokAuthHandler isolates authentication concerns from business logic and centralizes how the Grok API key is applied. As part of the HttpClient pipeline, it guarantees that all requests from the configured client include the Authorization header when a key is configured, without requiring per-request boilerplate. The use of `IOptionsMonitor<GrokOptions>` enables dynamic key rotation at runtime, so updated credentials take effect without restarting services.

## Notes
- Ensure GrokOptions is configured and that SectionName points to a live configuration source; otherwise, apiKey may remain empty and no Authorization header will be added.
- The handler only injects a Bearer header when a non-empty key is present; requests to endpoints that should be anonymous can proceed without authentication when the key is missing.
