Performs a web search against DuckDuckGo using the library's HTTP client and parsing helpers. It first attempts the richer html/ endpoint (which yields snippets and redirect-wrapped URLs) and, if that fails to produce parseable results or returns an anomaly/block page, falls back to the simpler lite/ endpoint. Returns a read-only list of parsed WebSearchResult entries, an empty list when the query truly has no results, or throws if both endpoints are blocked as bot traffic.

## Remarks
This method encapsulates the endpoint selection and anti-bot handling for DuckDuckGo scraping: it creates an HttpClient from the factory, ensures a warmed session, fetches and parses the html/ endpoint, then falls back to lite/ if needed. It detects anomaly/block pages and, if both endpoints indicate blocking, clears the session state (cookies/UA) via ResetSession() and throws an InvalidOperationException with guidance about switching to supported paid backends (brave or tavily). Extensive logging is produced for diagnostics (including the first 200 chars of responses when parsing yields zero results).

## Example
```csharp
// Typical usage inside an async method
var results = await duckDuckGoWebSearch.SearchAsync("how to make sourdough", 10, cancellationToken);
if (results.Count == 0) Console.WriteLine("No results found for that query.");
```

## Notes
- If both html/ and lite/ endpoints return an anomaly/block page the method throws InvalidOperationException; the exception message recommends switching to paid APIs (brave or tavily) and explains that free DDG endpoints are rate-limited.
- A returned empty list means the endpoints were reachable and parsed successfully but no results matched the query — this is distinct from the method throwing due to bot blocking.
- The method uses session state and may call ResetSession() when blocked; concurrent calls on the same instance could race with session-reset behavior, so avoid parallel calls on the same search client instance when possible.
- CancellationToken is observed for the async operations; operations may be cancelled by passing a cancelled token.
