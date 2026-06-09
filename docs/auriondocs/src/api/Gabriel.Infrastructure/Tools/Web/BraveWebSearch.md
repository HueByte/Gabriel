Implements IWebSearch using the Brave Search HTTP API. Use this when you want web search results sourced from Brave and prefer the HTTP client + API key to be provided via dependency injection (the class expects a named HttpClient "BraveSearch"). The implementation escapes the query, clamps the requested result count to the range 1–10, and maps the Brave JSON response into WebSearchResult instances.

## Remarks
This class encapsulates the HTTP call and JSON shape mapping so callers only deal with IWebSearch and WebSearchResult. It expects the HttpClient named "BraveSearch" to be configured elsewhere (base address, timeout and the X-Subscription-Token header / API key are configured in DI). Errors from the remote API surface as HttpRequestException after a warning is logged; there are no built-in retries or paging in this adapter.

## Example
```csharp
// Resolve via DI and call
var results = await braveWebSearch.SearchAsync("openai gpt-4", 5, CancellationToken.None);
foreach (var r in results)
{
    Console.WriteLine($"{r.Title} - {r.Url}\n{r.Snippet}\n");
}
```

## Notes
- If BraveSearchOptions.IsConfigured is false the method throws InvalidOperationException indicating the API key is missing.
- The requested limit is clamped to 1..10; requests for more than 10 will return at most 10 results.
- Non-success HTTP responses are logged (warning) and cause an HttpRequestException to be thrown; the response body is included in the log for diagnosis.
- If the JSON payload or its "web.results" section is absent the method returns an empty list rather than null.
- CancellationToken is passed through to HTTP and JSON reads; callers can cancel the request.
- The adapter does not implement retries or backoff; add a delegating handler or Polly policies on the named HttpClient if needed.
