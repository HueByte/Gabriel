# BraveWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/BraveWebSearch.cs`  
> **Kind:** class

Performs web searches against the Brave Search API and returns results as a list of WebSearchResult. Reach for this implementation when you want an IWebSearch backed by Brave and have configured a named HttpClient (HttpClientName = "BraveSearch") with the API base address, timeout and X-Subscription-Token authentication header in your DI setup.

## Remarks
A thin adapter over Brave's GET /search endpoint: it constructs an escaped query URL with a count parameter (clamped to 1–10), issues the request using IHttpClientFactory.CreateClient("BraveSearch"), deserializes the minimal JSON shape required, and projects Brave's results into WebSearchResult DTOs. Keeping authentication and connection settings on the named HttpClient centralizes configuration and avoids leaking API keys into callers.

## Example
```csharp
// Assume IWebSearch is registered and resolved from DI
var results = await webSearch.SearchAsync("open source databases", 5, CancellationToken.None);
foreach (var r in results)
{
    Console.WriteLine($"{r.Title} - {r.Url}\n{r.Snippet}\n");
}
```

## Notes
- If BraveSearchOptions.IsConfigured is false, SearchAsync throws InvalidOperationException; ensure the API key is present in configuration before calling.
- The requested limit is clamped to 1..10; passing a larger value will not increase the number of results returned from Brave.
- On non-success HTTP responses the response body is logged at Warning level and SearchAsync throws HttpRequestException; the response body is not included in the exception.
- JSON deserialization may produce null payloads; the implementation treats missing payload/results as an empty result set.
- The provided CancellationToken is passed to both the HTTP request and JSON read operations.