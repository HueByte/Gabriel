SearchAsync orchestrates a resilient web search against DuckDuckGo by first attempting the rich HTML endpoint and then falling back to the lite endpoint if necessary. It returns up to the specified limit as a read-only list of WebSearchResult. If both endpoints indicate bot-blocking/anomaly, it resets the session and throws InvalidOperationException to signal that scraping is blocked and to guide the caller toward a supported API key-based approach. When results exist, it returns them; when neither endpoint yields results, it returns an empty list.

## Remarks
This abstraction hides the endpoint selection, anomaly handling, and session management behind a single method. It ensures consistent logging and fallback behavior, and centralizes the bot-detection flow so callers don't duplicate that logic. By encapsulating the dual-endpoint strategy, it provides a stable, testable surface for consuming code while reacting gracefully to external scraping defenses.

## Example
```csharp
var results = await duckDuckGo.SearchAsync("open source licenses", 10, cancellationToken);
```

## Notes
- This method may throw InvalidOperationException if both the html/ and lite/ endpoints return an anomaly/block page, signaling the caller to switch to a supported API-based approach.
- The limit parameter caps the number of results requested from the endpoints and influences parsing decisions; higher limits may incur more network usage.
- External factors (network conditions, bot protection, and endpoint availability) can cause the method to return an empty list even when relevant results exist.
