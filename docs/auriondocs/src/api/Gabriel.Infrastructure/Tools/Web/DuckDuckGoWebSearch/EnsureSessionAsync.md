Performs a one-time, session-scoped warm-up request to DuckDuckGo's homepage so the HttpClient's cookie store and a session User-Agent are initialized before making search requests. Call this before issuing queries when you need the service to mimic a real browser navigation (homepage load followed by searches) to avoid cold-request heuristics on DDG.

## Remarks
This method is idempotent and thread-safe: it uses an internal semaphore to ensure only one warm-up runs per process/session and sets a flag once complete. It picks and freezes a single User-Agent for the session (to avoid rapid UA churn) and issues a GET to the configured Homepage URL. The method intentionally ignores the response body—the goal is to populate the CookieContainer (and any server-set session state) as a side effect. Failures during the warm-up are non-fatal and are logged; the subsequent search request will still proceed but without the homepage cookies.

## Example
```csharp
// Ensure the HttpClient used has a CookieContainer on its handler so cookies from the
// homepage request are preserved for subsequent search requests.
await EnsureSessionAsync(httpClient, cancellationToken);
// Now perform search requests which will reuse the warmed session cookies and UA.
```

## Notes
- Ensure the HttpClient instance uses an HttpClientHandler (or equivalent) configured with a CookieContainer; otherwise the homepage request will not populate cookies for later requests.
- CancellationToken is observed during the semaphore wait and the HTTP request; if cancelled, the method will throw OperationCanceledException and no warm-up will be completed.
- A failed warm-up is logged but not thrown for other exceptions (they are caught and logged), so callers should not rely on it as a hard precondition.