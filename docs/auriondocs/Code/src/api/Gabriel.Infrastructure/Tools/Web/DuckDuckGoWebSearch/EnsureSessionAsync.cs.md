Ensures a DuckDuckGo session is established by performing a one-time GET to the homepage for the current session. The method acts as a lazy initializer: on the first call it acquires a lock, selects a stable User-Agent from the pool, sends a request to the homepage, and relies on the CookieContainer to capture session cookies. The body of the response is ignored; cookies set by the round trip are the signal that the session is ready for subsequent searches across html.* and lite.* subdomains. If the warm-up fails due to non-cancelled exceptions, the failure is logged but the actual search proceeds without the cookies; the session is marked as warmed to avoid repeated attempts.

## Remarks
The method centralizes the initial navigation effects (cookies, UA) so that subsequent search requests can assume a ready session. It helps mimic a real browser startup to reduce heuristics detection and keeps the one-time side effects isolated from higher-level search logic. The concurrency guard guarantees a single initialization per session even under concurrent invocations.

## Notes
- Warm-up is best-effort: non-cancelled failures are swallowed after logging and do not abort the subsequent search.
- Cookies collected during this round-trip are scoped to .duckduckgo.com and apply automatically to both html.* and lite.* subdomains via the CookieContainer.
- A single initialization per session is enforced by the combination of the _sessionWarmed flag and the _sessionLock.