# GrokAuthHandler

> **File:** `src/api/Gabriel.Infrastructure/Providers/GrokAuthHandler.cs`  
> **Kind:** class

```csharp
internal sealed class GrokAuthHandler : DelegatingHandler
```


GrokAuthHandler attaches the Grok API key as a Bearer token to every outbound request sent through the named Grok HttpClient. It reads the key from `IOptionsMonitor<GrokOptions>` so a rotated key (for example updated via Infisical and a reload signal) takes effect without recycling the pooled handler.

## Remarks
Centralizes authentication for all Grok HTTP calls by turning a dynamic API key into a standard Authorization header on the client side. By using IOptionsMonitor, it supports hot-reloading of credentials without recreating HttpClient instances. As a DelegatingHandler, it participates in the HttpClient pipeline and ensures every request is authenticated consistently.

## Notes
- The Authorization header is added only when the API key is non-empty; otherwise, requests proceed without an Authorization header.
- The API key is read from options at request time, so it may change over the lifetime of the client; avoid logging the key or its value.
- Ensure GrokOptions are registered with IOptionsMonitor and that this handler is part of the named HttpClient's handler chain.