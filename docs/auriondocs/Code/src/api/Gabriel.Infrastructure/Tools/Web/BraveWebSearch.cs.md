BraveWebSearch is a concrete implementation of IWebSearch that delegates to the Brave Search API. It uses a named HttpClient (BraveSearch) that is configured via DI so the base address, timeout, and the API key header are centralized for reuse. In SearchAsync, BraveWebSearch requires BraveSearchOptions.IsConfigured to be true; if not, it throws InvalidOperationException. It escapes the query, clamps the limit to 1–10, performs the GET, and on a successful response deserializes the JSON payload into a BraveSearchResponse. It then takes the web.results array (or an empty list if missing) and maps each BraveResult to a WebSearchResult by copying Title to Title, Url to Url, and Description to Snippet, using empty strings as fallbacks when fields are missing.

## Remarks
This abstraction isolates the Brave API specifics behind an IWebSearch implementation, enabling swapping providers or testing with mocks without changing consumers. It centralizes error handling (e.g., API key missing, non-success responses) and ensures a consistent WebSearchResult shape across providers.

## Notes
- Requires BraveSearchOptions.IsConfigured; otherwise InvalidOperationException is thrown.
- The query is escaped and the limit is clamped to 1–10; non-success HTTP responses are logged and result in an HttpRequestException.
- The payload mapping is null-safe; missing fields default to empty strings and a missing payload yields an empty results set.