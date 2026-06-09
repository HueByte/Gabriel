Resets the in-memory indicators that a web session has been "warmed" and the chosen user-agent so the next SearchAsync attempt will rebuild the session. Use this when DuckDuckGo returns an anomaly page (or similar unexpected response) and you want the next request to select a fresh user-agent and trigger a homepage round-trip to refresh cookies and slightly alter the client fingerprint.

## Remarks
This utility method exists to force a soft session restart without clearing persistent state or tearing down connection objects. By clearing only the warmed flag and stored user-agent, it causes the higher-level search flow to reinitialize the session (picking a new UA and performing the homepage request) which refreshes the cookie jar and can recover from DDG-provided anomalies or fingerprint-based blocking.

## Notes
- ResetSession only modifies the in-memory flags (_sessionWarmed and _sessionUserAgent). It does not clear cookies, dispose HTTP clients, or otherwise purge other session-related state — the actual cookie refresh happens later when the session is rebuilt and the homepage round-trip occurs.
- The method is private and contains no synchronization. If callers may invoke it from multiple threads, ensure appropriate external synchronization to avoid race conditions.