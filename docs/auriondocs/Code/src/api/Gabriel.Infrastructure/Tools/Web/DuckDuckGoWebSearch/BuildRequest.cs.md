BuildRequest creates a HttpRequestMessage configured with a browser-like header set to mimic a real navigation. It selects a per-session User-Agent (falling back to the first entry if none is set), applies standard Accept headers, and uses Sec-Fetch semantics that distinguish an initial navigation from an in-site hop, enabling session pinning and correct referer handling.

## Remarks
By centralizing header assembly, this method ensures that each outgoing request carries a consistent fingerprint and correct navigation context, without polluting HttpClient.DefaultRequestHeaders across multiple requests. The per-request approach makes it easier to rotate user agents and vary the referer as the user progresses from warmup to an actual search.

## Notes
- Falls back to UserAgents[0] when _sessionUserAgent is not populated.
- Referer header is only set for non-initial navigations; ensure Homepage is a valid URL to be used as the Referer.
- Some servers may ignore or override browser-like headers; callers should not rely on these headers for strict security policies.