Builds an HttpRequestMessage pre-populated with the set of request headers that mimic a real browser navigation (Chrome/Firefox). Use this when sending navigational requests to DuckDuckGo so each request carries realistic User-Agent, Sec-Fetch, Referer and related headers — and so the User-Agent can be pinned/rotated per session instead of relying on global DefaultRequestHeaders.

## Remarks
This method creates headers on a per-request basis rather than using HttpClient.DefaultRequestHeaders so session-scoped User-Agent values can be applied and so the Sec-Fetch-Site/Referer pair can vary between an "initial navigation" (no referer, Sec-Fetch-Site: none) and subsequent in-site navigations (Referer set to the configured Homepage and Sec-Fetch-Site: same-site). It intentionally mirrors browser navigation signals to reduce bot detection and to produce more realistic traffic patterns.

## Example
```csharp
// Build a request for the initial warm-up navigation
var request = BuildRequest(HttpMethod.Get, "https://duckduckgo.com/", isInitialNavigation: true);
var response = await _httpClient.SendAsync(request);

// Build a request for a follow-up search/navigation (sets Referer to Homepage)
var searchRequest = BuildRequest(HttpMethod.Get, "https://duckduckgo.com/?q=test", isInitialNavigation: false);
var searchResponse = await _httpClient.SendAsync(searchRequest);
```

## Notes
- The method sets common navigation headers but does not manage cookies; cookie handling is expected to be performed by the HttpClient/HttpClientHandler used to send the request.
- _sessionUserAgent is used when available; otherwise the first entry from UserAgents is used as a defensive fallback. EnsureSessionAsync is expected to populate the session UA before calls reach this method.
- Referer and Sec-Fetch-Site are conditional: initial navigations omit Referer and set Sec-Fetch-Site to "none", while subsequent navigations set Referer to Homepage and Sec-Fetch-Site to "same-site".
- Header values are hard-coded to emulate browser behavior; change them deliberately if you need different fetch semantics.