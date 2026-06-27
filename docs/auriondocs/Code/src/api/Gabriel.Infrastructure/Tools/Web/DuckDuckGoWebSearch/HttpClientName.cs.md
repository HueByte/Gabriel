A central, canonical name for the named HttpClient used to call DuckDuckGo search endpoints. Use this constant when registering or resolving the HttpClient in dependency injection to avoid hard-coded strings and typos.

## Remarks
This constant serves as the single source of truth for the DI registration name of the HttpClient that performs DuckDuckGo web searches. Keeping the name in one place reduces copy/paste errors and makes it easier to refactor or locate the client registration.

## Example
```csharp
// Registering the named HttpClient in Startup/Program
services.AddHttpClient(HttpClientName, client => {
    client.BaseAddress = new Uri("https://duckduckgo.com/");
    // configure other defaults (timeouts, headers) here
});

// Resolving the named client via IHttpClientFactory
var client = httpClientFactory.CreateClient(HttpClientName);
```

## Notes
- Because this is a const, its value is a compile-time constant and may be inlined into referencing assemblies. If you change the string literal, dependent assemblies should be rebuilt to pick up the new value.
- Ensure the registration key used when calling AddHttpClient exactly matches this constant; mismatches will result in runtime failures to resolve the intended client.