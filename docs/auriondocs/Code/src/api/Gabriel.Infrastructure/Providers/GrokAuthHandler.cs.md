# GrokAuthHandler

> **File:** `src/api/Gabriel.Infrastructure/Providers/GrokAuthHandler.cs`  
> **Kind:** class

Attaches the configured Grok API key as a Bearer token to outbound HTTP requests for a named or typed HttpClient. Reach for this handler when you want every request from a particular HttpClient instance to include the current API key and you need key rotations (via configuration or a secrets manager) to take effect immediately without recycling the pooled handler.

## Remarks
This is a DelegatingHandler intended to be wired into an IHttpClientFactory pipeline (for example, with AddHttpMessageHandler). It reads GrokOptions through IOptionsMonitor so the handler observes the latest API key on each SendAsync invocation; that design lets rotated keys become effective immediately rather than relying on handler or client recreation.

## Example
```csharp
// Register options and the handler, then attach the handler to a named client
services.Configure<GrokOptions>(configuration.GetSection("Grok"));
services.AddTransient<GrokAuthHandler>();

services.AddHttpClient("grok-client")
        .AddHttpMessageHandler<GrokAuthHandler>();

// Usage
var client = httpClientFactory.CreateClient("grok-client");
var response = await client.GetAsync("/some/grok/endpoint");
```

## Notes
- The handler sets the Authorization header on each request; if another handler or caller has already set an Authorization header earlier in the pipeline it will be overwritten.
- Ensure GrokOptions (or whatever configuration supplies the API key) is wired into DI so IOptionsMonitor can provide the latest value.
- Register the handler as a transient service (or use AddHttpMessageHandler) so it participates correctly in the HttpClientFactory handler pipeline and reads fresh option values per request.