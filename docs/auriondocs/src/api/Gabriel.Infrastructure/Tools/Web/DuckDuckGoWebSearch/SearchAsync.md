Performs a DuckDuckGo web search for the given query and returns up to `limit` results as an IReadOnlyList<WebSearchResult>. The method first attempts the richer html/ endpoint (which provides snippets and redirect-wrapped URLs) and, if that endpoint is blocked or yields no parseable results, falls back to the lighter lite/ endpoint. The provided CancellationToken is passed through to network calls so the operation can be canceled.

## Remarks
This method is written to be resilient against DuckDuckGo's anti-bot behavior: it calls EnsureSessionAsync to prepare a client (UA/cookies), prefers the html/ endpoint for richer results, and uses the lite/ endpoint as a less aggressive fallback. If either endpoint returns an anomaly/CAPTCHA page the method logs a warning; if both endpoints return an anomaly it clears the session state (ResetSession) and throws an InvalidOperationException to indicate the request was blocked as bot traffic. When both endpoints return valid HTML but no parseable results, the method returns an empty list rather than throwing.

## Example
```csharp
// Typical usage
var results = await duckDuckGoWebSearch.SearchAsync("open source licenses", 10, CancellationToken.None);
foreach (var r in results)
{
    Console.WriteLine(r.Title + " -> " + r.Url);
}

// Handling the case where DDG blocks the client
try
{
    var results = await duckDuckGoWebSearch.SearchAsync("some query", 20, ct);
}
catch (InvalidOperationException ex)
{
    // DDG returned anomaly/CAPTCHA pages on both endpoints; consider using a supported paid provider
    Console.Error.WriteLine(ex.Message);
}
```

## Notes
- If both html/ and lite/ endpoints return an anomaly (bot/CAPTCHA) page the method resets session state and throws InvalidOperationException — subsequent calls will re-warm with a different UA/cookie jar.
- The method may return fewer than `limit` results if fewer parseable results are available for the query.
- Network errors and cancellation will propagate (e.g., HttpRequestException, OperationCanceledException); logs include snippets/lengths of returned HTML to aid debugging.