Ensures the DuckDuckGo session is initialized by performing a lightweight GET to the homepage to obtain cookies and establish a stable User-Agent for subsequent requests. It runs once per session and uses a lock to prevent concurrent warm-ups; cookies from this request are scoped to .duckduckgo.com and apply to both html.* and lite.* subdomains.

## Remarks
The method encapsulates a session warming step that prepares cookies and a consistent User-Agent for subsequent requests, mirroring a browser's initial navigation to the site. It is deliberately best-effort and idempotent; if the warm-up fails (other than cancellation), the real search will still be attempted, but without homepage cookies, and a warning is logged. The lock-guarded, one-time nature ensures only a single warm-up occurs per session, avoiding redundant round-trips when multiple callers race to initialize.

## Notes
- Cancellation propagates: OperationCanceledException is not swallowed, so cancellation during warm-up will propagate to the caller.
- No guarantee of cookies: if a non-cancellation failure occurs, cookies may not be established, but the actual search request will still be issued.
- One-time per session: after the initial successful run, subsequent invocations return immediately thanks to _sessionWarmed.