Performs an HTTP GET against DuckDuckGo for the provided query and returns the response body as a string. This method introduces a small randomized delay and URL-escapes the query to better mimic human navigation; use it from higher-level search code when you need the raw HTML result page for a query.

## Remarks
This helper is designed to reduce bot-detection signals: it waits a random 200–1200ms before issuing the request and uses a GET with a query string (not a POST with a form body) to more closely resemble a user typing a URL and pressing Enter. It builds and disposes the HttpRequestMessage and HttpResponseMessage (via using), logs non-successful responses, and throws an HttpRequestException for non-success status codes. A CancellationToken is respected for the delay, request, and content read operations.

## Example
```csharp
// Typical usage within an async method
using var http = new HttpClient();
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
string html = await FetchAsync(http, "https://lite.duckduckgo.com/lite", "how to bake sourdough", cts.Token);
// html now contains the page HTML returned by DuckDuckGo
```

## Notes
- The method throws HttpRequestException when the response status is not successful; callers should handle or propagate this.
- The built-in randomized delay (200–1200ms) introduces nondeterminism — avoid calling directly from fast unit tests or inject/test around the delay in integration tests.
- The query is URL-escaped and the method appends the parameter `kl=us-en`; do not include the q or kl parameters in the provided url.
- CancellationToken cancels Task.Delay, the SendAsync call, and the subsequent ReadAsStringAsync.
- Request construction (headers, Sec-Fetch-* etc.) is delegated to BuildRequest; callers should be aware that that method influences headers and navigation semantics.
```