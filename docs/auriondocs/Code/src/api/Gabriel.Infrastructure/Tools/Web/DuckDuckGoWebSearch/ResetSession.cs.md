Resets the current DuckDuckGo session state used during automated searches. When invoked after detecting an anomaly page, it clears the warmed flag and the cached User-Agent so the next search rebuilds the HTTP session with a freshly chosen UA. This triggers another homepage round-trip, refreshing the cookie jar provided by DDG and subtly altering the request fingerprint for subsequent interactions.

## Remarks
Encapsulates the session-lifecycle recovery strategy in one place, decoupling anomaly handling from the normal search flow. It enables retry-with-new-session behavior instead of continuing with a potentially stale or blocked session; UA rotation and cookie refresh are tied to this reset to improve resilience against anti-bot measures.

## Notes
- Private to the class; should be invoked only by internal anomaly-handling code.
- The reset does not immediately delete cookies; a subsequent network call must occur to refresh cookies.
- Calls are effectively idempotent; repeated invocations simply ensure the session is in a clean initial state before the next request.