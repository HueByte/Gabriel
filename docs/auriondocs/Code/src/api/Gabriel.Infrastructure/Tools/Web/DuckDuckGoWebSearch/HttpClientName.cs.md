HttpClientName defines the canonical name used when registering and obtaining a named HttpClient for the DuckDuckGo web search integration. Use this constant to avoid hard-coded strings and keep all HttpClientFactory registrations and lookups in sync.

## Remarks
This constant centralizes the client name so that multiple parts of the application don't drift apart in their DI usage. It aligns with the HttpClientFactory pattern and improves testability by making the dependency explicit and reusable across registration and consumption sites.

## Example
```csharp
// Register a named HttpClient for DuckDuckGo-related calls
services.AddHttpClient(HttpClientName, client => {
    client.BaseAddress = new Uri("https://api.duckduckgo.com/");
    // configure defaults (headers, timeouts, etc.) as needed
});

// Consume the named client via IHttpClientFactory
public class DuckDuckGoSearcher
{
    private readonly HttpClient _httpClient;

    public DuckDuckGoSearcher(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientName);
    }

    // Use _httpClient to perform requests
}
```

## Notes
- This is a compile-time constant; changing its value requires recompiling all referencing code.
- Ensure consistent use of HttpClientName in both AddHttpClient registrations and CreateClient calls to avoid runtime misconfigurations.