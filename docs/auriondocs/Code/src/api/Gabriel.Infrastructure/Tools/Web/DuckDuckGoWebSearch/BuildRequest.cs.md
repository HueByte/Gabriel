Constructs an HttpRequestMessage that closely emulates the set of headers a real Chrome/Firefox navigation would send. Use this when performing HTTP fetches against DuckDuckGo where the request should appear as a browser navigation — for example, distinguishing an initial (entered/bookmarked) navigation from an in-site navigation so Sec-Fetch-Site and Referer are set appropriately, and so the User-Agent can be session-pinned or rotated per session.

## Remarks
This method deliberately builds headers on a per-request basis rather than relying on HttpClient.DefaultRequestHeaders. That lets the implementation: (a) pin or rotate the User-Agent at the session level, and (b) vary Sec-Fetch-Site and Referer between the initial warm-up navigation and subsequent same-site navigations. The method sets common navigation headers (Accept, Accept-Language, DNT, Upgrade-Insecure-Requests, Sec-Fetch-*) and falls back to the first configured user-agent when the session user-agent is null.

## Example
```csharp
// Initial navigation (no Referer, Sec-Fetch-Site: none)
var request = BuildRequest(HttpMethod.Get, "https://duckduckgo.com/", isInitialNavigation: true);
var response = await httpClient.SendAsync(request);

// Subsequent navigation (Referer set to Homepage, Sec-Fetch-Site: same-site)
var nextRequest = BuildRequest(HttpMethod.Get, "https://duckduckgo.com/?q=example", isInitialNavigation: false);
var nextResponse = await httpClient.SendAsync(nextRequest);
```

## Notes
- The method does not manage cookies; cookie handling (if any) is expected to be done by the HttpClient/handler or elsewhere in the codebase.
- If _sessionUserAgent is null the code falls back to UserAgents[0]; callers should ensure session initialization (e.g. EnsureSessionAsync) runs before relying on a session-specific UA.
- Adding header values with invalid format may throw (FormatException); header values supplied here follow common browser formats but malformed inputs will propagate errors.
- Do not reuse a single HttpRequestMessage instance for multiple sends; create a fresh request per call as this method does.