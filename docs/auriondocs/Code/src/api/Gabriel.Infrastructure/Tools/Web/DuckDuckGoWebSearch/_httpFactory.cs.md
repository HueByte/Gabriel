Holds the IHttpClientFactory instance used by this class to create HttpClient objects for making outbound HTTP requests (for example, sending DuckDuckGo search queries). The factory is intended to be supplied by dependency injection and used by instance methods instead of instantiating HttpClient directly.

## Remarks
This field centralizes HTTP client creation and configuration for the class, allowing the application to reuse handlers, avoid socket exhaustion, and apply shared settings (timeouts, default headers, message handlers, or named client configuration) via the DI-provided factory. Keeping the factory as a readonly field enforces that the injected dependency is not reassigned after construction.

## Example
```csharp
// inside a method of DuckDuckGoWebSearch
var client = _httpFactory.CreateClient();
var response = await client.GetAsync(requestUri);
response.EnsureSuccessStatusCode();
var content = await response.Content.ReadAsStringAsync();
```

## Notes
- The IHttpClientFactory is normally provided by the application's dependency injection container; ensure the service is registered (e.g., services.AddHttpClient()).
- Prefer configuring named or typed clients on the factory for per-client settings rather than mutating HttpClient instances at call sites.
- The factory manages handler lifetimes to avoid socket exhaustion; disposing the created HttpClient is allowed but typically unnecessary when using the factory.