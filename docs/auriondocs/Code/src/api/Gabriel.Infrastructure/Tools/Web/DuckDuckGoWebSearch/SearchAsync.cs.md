Performs a DuckDuckGo web search using the class's HTTP session management. The method first attempts the richer html/ endpoint (which yields snippets and redirect-wrapped URLs) and, if that fails to produce parseable results or returns an anomaly/block page, falls back to the simpler lite/ endpoint. It returns up to the requested number of parsed WebSearchResult items; if both endpoints return valid HTML but no results, an empty list is returned. If both endpoints return an anomaly/block page the method clears the current session state (so the next call will re-warm with a different UA and cookie jar) and throws an InvalidOperationException explaining that DuckDuckGo has flagged the request as bot traffic.

## Remarks
This method centralizes endpoint selection and anti-bot handling for DuckDuckGo lookups: it prefers the richer html/ endpoint for best results but uses the lite/ endpoint as a resilient fallback because its markup is simpler and it is less likely to trigger DDG's bot-detection. The ResetSession call is an intentional side effect to rotate the client fingerprint (user-agent and cookies) when DDG indicates blocking; callers should treat this as a stateful operation on the search client.

## Notes
- Throws InvalidOperationException when both html/ and lite/ endpoints return an anomaly/block page; the exception message recommends using paid/official search tools for reliable access.
- Returns Array.Empty<WebSearchResult>() when both endpoints return parseable HTML but no results (a genuine empty result set).
- The method logs informational and warning messages about parsing outcomes and detected anomaly pages; inspect logs for troubleshooting.
- The cancellation token is passed to network operations; operations will observe cancellation where supported.
- The limit parameter is applied by the HTML/Lite parsing routines to cap returned results; the method itself does not retry beyond the two endpoints.