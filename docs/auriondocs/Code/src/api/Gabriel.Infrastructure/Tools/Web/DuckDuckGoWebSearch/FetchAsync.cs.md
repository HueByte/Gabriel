FetchAsync is a private helper that retrieves the HTML content for a given search query from DuckDuckGo. It performs a small, randomized delay before issuing a GET request with the query encoded as a URL parameter and returns the response body as a string; if the HTTP response is not successful, it logs a warning and throws HttpRequestException.

## Remarks
FetchAsync encapsulates the low-level HTTP fetch for a DuckDuckGo search, including query encoding, URL construction, and basic error handling. The intentional delay and navigation framing aim to mimic human browsing cadence and align request headers with a real user navigation, so higher-level search orchestration can remain agnostic to anti-bot considerations.

## Notes
- No retry logic is implemented; failures surface as HttpRequestException for the caller to handle.
- The delay is cancelable via the provided CancellationToken (ct).
- The request uses a GET with query parameters (q and kl) and escapes the query, ensuring the URL is well-formed for the target endpoint.