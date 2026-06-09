# GrokAuthHandler

> **File:** `src/api/Gabriel.Infrastructure/Providers/GrokAuthHandler.cs`  
> **Kind:** class

Attaches the current Grok API key as a Bearer token to outbound HTTP requests sent through the Grok-named HttpClient. Use this handler when you want requests to automatically include authentication and to pick up rotated/updated API keys at runtime without recycling the pooled HttpClient handler.

## Remarks
This DelegatingHandler reads the Grok API key via `IOptionsMonitor<GrokOptions>` at request time so updates to configuration (for example a rotated secret) take effect immediately for subsequent requests. It is intended to be added to a named HttpClient pipeline (via AddHttpMessageHandler or similar) so authentication is centralized and callers can use IHttpClientFactory-created clients without manually adding Authorization headers.

## Example
```csharp
// Startup / DI registration
services.Configure<GrokOptions>(configuration.GetSection("Grok"));
services.AddTransient<GrokAuthHandler>();
services.AddHttpClient("Grok", client => {
    client.BaseAddress = new Uri("https://api.grok.example");
})
.AddHttpMessageHandler<GrokAuthHandler>();

// Consuming code
var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
var grokClient = clientFactory.CreateClient("Grok");
var response = await grokClient.GetAsync("/v1/some-endpoint");
```

## Notes
- The handler will overwrite an existing Authorization header on the outgoing request; ensure callers do not need to set a different credential on the same HttpRequestMessage.
- Because the handler reads the key at send time (via IOptionsMonitor), avoid caching the secret in the handler instance — the pattern intentionally uses the monitor to allow key rotation without recycling handlers.
- Treat the API key as a secret: do not log request headers or otherwise persist the key, and ensure HTTPS/TLS is used for outgoing requests.