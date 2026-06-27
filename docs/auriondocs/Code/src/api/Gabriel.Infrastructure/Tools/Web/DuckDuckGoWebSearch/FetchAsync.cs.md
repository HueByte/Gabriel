Sends a single, human-like DuckDuckGo search navigation and returns the response body.

This method performs a randomized small delay, builds a GET request using the provided base URL and query (appending q and kl=us-en), and sends the request via the provided HttpClient. It uses BuildRequest(...) to create the HttpRequestMessage (so navigation-like headers can be applied), verifies a successful HTTP status, and returns the response content as a string.

## Remarks
This helper exists to reduce bot-detection signals and to emulate a real user navigation: it inserts a 200–1200ms randomized delay, uses an HTTP GET with the query in the URL rather than a POST form body, and relies on BuildRequest to attach Sec-Fetch-* and other headers consistent with a user-typed navigation. On non-success status codes the method logs a warning and throws an HttpRequestException so callers can treat failures uniformly.

## Notes
- The method will throw HttpRequestException for non-success HTTP responses and will propagate other exceptions from HttpClient operations.
- The query parameter is escaped with Uri.EscapeDataString; the base url is concatenated with a ?q=... string — do not pass a url that already contains a query string or you may create an invalid/incorrect request URI.
- CancellationToken is honored for the initial delay, the HTTP send, and the content read; callers can cancel to abort the operation.