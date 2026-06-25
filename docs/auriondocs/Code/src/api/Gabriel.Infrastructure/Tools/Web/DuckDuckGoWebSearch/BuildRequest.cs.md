BuildRequest constructs an HttpRequestMessage configured with a browser-like header set to drive realistic navigation requests against the DuckDuckGo web surface. It builds per-request headers rather than using HttpClient.DefaultRequestHeaders so the User-Agent can be session-pinned and the Sec-Fetch headers can reflect whether the navigation is an initial warmup or a subsequent in-site navigation. The returned HttpRequestMessage is ready to be sent via HttpClient and mirrors the behavior of a real browser request, including a conditional Sec-Fetch-Site/Referer depending on whether isInitialNavigation is true.

## Remarks
Encapsulates the header-crafting logic in one place, reducing drift across requests that fetch pages and enabling consistent session behavior. It clarifies why header variation is necessary: the initial navigation uses Sec-Fetch-Site: none and no Referer, while later navigations use same-site and a Referer of Homepage. This centralization also makes testing navigation behavior easier by constraining how per-request headers are formed.

## Notes
- Ensure UserAgents is non-empty and _sessionUserAgent is populated before calling; otherwise the expression _sessionUserAgent ?? UserAgents[0] may throw when indexing UserAgents[0].
- The method is private; callers must interact via higher-level navigation flows that call BuildRequest internally.
- The headers include values that may be strict or bot-detection–heavy for some servers; adjust header values if targeting hosts with stricter detection.