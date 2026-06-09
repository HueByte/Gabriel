Constructs an HttpRequestMessage pre-populated with the set of headers a real browser navigation (Chrome/Firefox) would send. Use this when issuing navigation-style HTTP requests so the remote site sees realistic Accept, User-Agent, Sec-Fetch-* and related headers; the method builds headers per-request so the user-agent can be session-pinned and the Sec-Fetch-Site/Referer can vary between an initial (external) navigation and an internal (same-site) navigation.

## Remarks
The method intentionally builds headers on each HttpRequestMessage rather than relying on HttpClient.DefaultRequestHeaders so the request can reflect session-scoped state and per-hop differences. If isInitialNavigation is true the request signals an external entry (Sec-Fetch-Site: none and no Referer). For subsequent navigations it signals an intra-site navigation (Sec-Fetch-Site: same-site) and sets Referer to the configured Homepage. The User-Agent header falls back to the first entry in the UserAgents pool when the session-specific _sessionUserAgent is null.

## Example
```csharp
// initial warmup/navigation (no Referer)
var req1 = BuildRequest(HttpMethod.Get, "https://duckduckgo.com/", isInitialNavigation: true);

// subsequent in-site navigation (Referer set to Homepage)
var req2 = BuildRequest(HttpMethod.Get, "https://duckduckgo.com/search?q=example", isInitialNavigation: false);

// both req1 and req2 are ready to send with realistic navigation headers
```

## Notes
- The method does not set cookies, authentication, or request content; callers must add those as needed before sending.
- The code falls back to UserAgents[0] when _sessionUserAgent is null; EnsureSessionAsync (or equivalent) is expected to set the session-scoped UA when applicable.
- Some intermediaries or HttpClient handlers may modify or strip certain headers; callers should validate the final outgoing request if precise header preservation is required.
