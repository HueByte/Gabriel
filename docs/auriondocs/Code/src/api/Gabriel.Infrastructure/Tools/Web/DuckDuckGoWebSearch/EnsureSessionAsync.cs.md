Performs a one-time, session-wide GET of DuckDuckGo's homepage to warm the client's cookie jar and commit to a single User-Agent string for the session. Call this before issuing search queries when you want the HttpClient to present a more realistic browser-like session (cookies and a stable UA) so search requests are less likely to be flagged by DuckDuckGo heuristics.

## Remarks
This method exists to mimic real browser behavior: real users navigate to the site first, acquiring session cookies and then submit search queries. Warming the session reduces the chance that a direct cold request to the search endpoint will be treated as scripted. It picks a single User-Agent from a predefined list for the lifetime of the warmed session, performs an HTTP GET to the configured homepage (the response body is intentionally ignored), and relies on the HttpClient's cookie handling to collect cookies for subsequent requests. A SemaphoreSlim-style lock ensures only one warm-up runs concurrently; once warmed the operation is skipped on future calls.

## Example
```csharp
// Ensure the HttpClient session is warmed before sending search requests
await EnsureSessionAsync(httpClient, cancellationToken);
// proceed to send search query using the same HttpClient
```

## Notes
- The method is resilient: any exception except OperationCanceledException is caught, logged as a warning, and treated as non-fatal — the application continues without the homepage cookies.
- Cancellation is respected via the provided CancellationToken; if cancelled the OperationCanceledException will propagate.
- Concurrent callers are serialized by an internal async lock; the warm-up runs at most once per process lifetime (per instance) as indicated by the _sessionWarmed flag.
- The response body is not read because the goal is to populate cookies; a non-200 status is acceptable so long as cookies are set.
- The chosen User-Agent is persisted for the warmed session and should not change between calls; rapid UA changes are avoided to reduce heuristic detection risk.
- Cookies are scoped to .duckduckgo.com so they apply across expected subdomains (for example html.* and lite.*).