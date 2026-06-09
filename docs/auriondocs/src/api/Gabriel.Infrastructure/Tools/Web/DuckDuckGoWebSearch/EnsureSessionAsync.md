Performs a one-time "warm up" navigation to the DuckDuckGo homepage to establish a session (cookies) and pick a stable User-Agent for the current session. Call this before issuing search requests when you want the HttpClient's CookieContainer to hold the same cookies a real browser would obtain by first visiting the homepage, which reduces the chance DDG flags the search as a cold/direct query.

## Remarks
This method is safe to call concurrently: it uses an internal lock to ensure only a single navigation happens per process/session and an internal flag to make the operation idempotent. The chosen User-Agent is committed to a session field so subsequent requests use a consistent UA. The actual HTTP response body is ignored — the intent is the side effect of cookies being added to the HttpClient's CookieContainer.

## Example
```csharp
// Ensure the HttpClient is configured with a CookieContainer (via HttpClientHandler)
await EnsureSessionAsync(httpClient, cancellationToken);
// Now perform search requests using the same httpClient so they carry the warmed cookies
var results = await PerformDuckDuckGoSearchAsync(httpClient, "example query", cancellationToken);
```

## Notes
- The HttpClient passed in must use a CookieContainer (HttpClientHandler) for cookies to be collected; otherwise the warm-up has no effect.
- Failures during the warm-up are non-fatal and logged; however, the method marks the session as warmed even after a failed request, so it will not retry on subsequent calls.
- Cancellation (OperationCanceledException) is propagated when waiting for the internal lock or if the SendAsync is canceled; other exceptions are caught and logged as warnings.