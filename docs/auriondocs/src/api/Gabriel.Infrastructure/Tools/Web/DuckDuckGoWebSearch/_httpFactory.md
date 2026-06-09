Holds the IHttpClientFactory instance provided to the DuckDuckGoWebSearch class via dependency injection. The field is used within the class to obtain configured HttpClient instances for making HTTP requests to DuckDuckGo or related web endpoints — prefer using this factory instead of creating raw HttpClient instances so the runtime can manage handler lifetimes and configuration.

## Remarks
Using IHttpClientFactory centralizes HTTP client configuration (policies, handlers, timeouts, named/typed clients) and avoids common issues like socket exhaustion that occur when callers create and dispose HttpClient instances frequently. The field is readonly and intended to be assigned in the constructor; other members of the class call CreateClient() on this factory to perform network I/O.

## Example
```csharp
// Typical constructor injection and usage inside the class
public DuckDuckGoWebSearch(IHttpClientFactory httpFactory)
{
    _httpFactory = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));
}

private async Task<string> FetchAsync(string url)
{
    var client = _httpFactory.CreateClient();
    var response = await client.GetAsync(url).ConfigureAwait(false);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
}
```

## Notes
- Ensure the DI container registers IHttpClientFactory (for example, via services.AddHttpClient()).
- Do not create a new HttpClient per request manually; prefer CreateClient() from the factory to avoid socket exhaustion and to reuse configured handlers.
- Clients returned by the factory are managed; disposing them is allowed but usually unnecessary — prefer to let the factory manage lifetimes.