This public constant contains the identifier used to configure and retrieve the HttpClient instance dedicated to the DuckDuckGo search workflow. Developers should reference HttpClientName when registering a named HttpClient with IHttpClientFactory and when obtaining that client, rather than scattering the literal string across the codebase. This centralizes the client configuration and reduces the risk of typos or mismatched names.

## Remarks
This name acts as a contract between DI configuration and its consumers. It enables swapping the underlying HttpClient configuration (for example, to point to a mock or test server) without changing call sites. It also makes tests easier by allowing controlled substitution of the named client.

## Example
```csharp
// Registration
services.AddHttpClient(HttpClientName, client =>
{
    client.BaseAddress = new Uri("https://duckduckgo.com/");
    // additional defaults (headers, timeouts, etc.)
});

// Consumption
public class DuckDuckGoService
{
    private readonly HttpClient _httpClient;
    public DuckDuckGoService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientName);
    }

    // use _httpClient for requests
}
```

## Notes
- Reference HttpClientName rather than the literal string to avoid typos; renaming the constant will propagate automatically.
- Ensure the registration and usage both rely on the same HttpClientName; a mismatch would result in a missing client at runtime.