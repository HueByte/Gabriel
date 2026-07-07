# GrokAuthHandler

> **File:** `src/api/Gabriel.Infrastructure/Providers/GrokAuthHandler.cs`  
> **Kind:** class

```csharp
internal sealed class GrokAuthHandler : DelegatingHandler
```


GrokAuthHandler is a DelegatingHandler that attaches the Grok API key as a Bearer token on every outbound HTTP request sent through a HttpClient. It reads the key via `IOptionsMonitor<GrokOptions>` so a rotated key (for example, one swapped via a configuration reload) takes effect without recycling the pooled handler.

## Remarks
By implementing the authentication logic as a DelegatingHandler, this symbol keeps per-request header management centralized and reusable across any HttpClient that targets the Grok API. The use of IOptionsMonitor enables runtime key rotation without tearing down HttpClient instances, aligning with HttpClientFactory patterns and DI-driven app lifecycles.

## Notes
- The Authorization header is only added when the API key is non-empty; otherwise, the request proceeds without a Bearer token.
- The API key is sourced from GrokOptions via IOptionsMonitor, enabling runtime rotation without recreating HttpClient or its handler chain.
- If GrokOptions.SectionName is misconfigured or the key is missing, callers may observe requests without authentication; ensure the configuration is loaded correctly (and that SectionName points to the proper configuration section).
