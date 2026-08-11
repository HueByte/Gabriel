FetchAsync is a compact helper that retrieves the HTML content for a DuckDuckGo search by performing an HTTP GET after a short, randomized delay. It takes an HttpClient, a base URL, a search query, and a CancellationToken; it builds a request URL with the query encoded as q and a fixed locale parameter (kl=us-en), sends the request, and returns the response body as a string. If the response indicates failure, it logs a warning and throws HttpRequestException containing the HTTP status, enabling callers to respond to transient network conditions.

## Remarks
This method centralizes the web-fetch logic used by the DuckDuckGo navigation flow, encapsulating URL construction, request sending, and error handling. The intentional random delay helps mimic human interaction cadence and reduces the likelihood of bot-detection signals when issuing rapid requests in sequence. By isolating these concerns behind a private helper, higher-level search logic remains concise, testable, and resilient to common HTTP errors.

## Notes
- The delay is cancellable via the provided CancellationToken; a cancellation will surface as a TaskCanceledException.
- Non-success HTTP responses yield an HttpRequestException that includes the status code; callers should implement appropriate retry or fallback strategies.
