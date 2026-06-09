A shared, compile-time constant string that identifies the named HttpClient used for DuckDuckGo web searches. Use this constant when registering the named client with dependency injection (AddHttpClient) or when creating/requesting the client from an IHttpClientFactory to avoid hard-coded string literals and typos.

## Remarks
Centralizes the HttpClient registration name so registration and consumption sites remain consistent. Keeping the identifier in one place reduces accidental mismatches when multiple components need the same named client.

## Example
```csharp
// Register the named HttpClient in Startup/Program
services.AddHttpClient(HttpClientName, client =>
{
    client.BaseAddress = new Uri("https://api.duckduckgo.com/");
    // other client configuration
});

// Later, resolve and use the named client
var client = httpClientFactory.CreateClient(HttpClientName);
// use client to perform DuckDuckGo search requests
```

## Notes
- Because this is a const, its value is inlined into referencing assemblies at compile time; changing the constant's value requires recompiling consumers.
- The constant is public so it can be referenced across assembly boundaries when multiple projects need the same named client.