A central, compile-time constant string used as the name/key for the HttpClient instance that performs DuckDuckGo web search requests. Reach for this constant when registering or resolving the named HttpClient so callers and registrations remain consistent and typo-free.

## Remarks
This constant is the single source of truth for the named HttpClient used by the DuckDuckGo web search tooling. It exists to avoid scattering the literal "DuckDuckGoSearch" identifier throughout the codebase and to ensure all registrations and lookups (e.g., with IHttpClientFactory or AddHttpClient) reference the same name.

## Example
```csharp
// Registering the named HttpClient during startup/configuration
services.AddHttpClient(HttpClientName, client =>
{
    client.BaseAddress = new Uri("https://duckduckgo.com/");
    // additional configuration (timeouts, default headers, etc.)
});

// Resolving and using the named client elsewhere
var client = httpClientFactory.CreateClient(HttpClientName);
var response = await client.GetAsync("/?q=example");
```

## Notes
- Being const means the value is a compile-time constant; changing it requires recompilation of callers.
- Any registration or CreateClient call must use this exact value; mismatched names will result in failing to resolve the intended configuration.
- Keep this constant in sync with any configuration or documentation that refers to the DuckDuckGo HttpClient name.