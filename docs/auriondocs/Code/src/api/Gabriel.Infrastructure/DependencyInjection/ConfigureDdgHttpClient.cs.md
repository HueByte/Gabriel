Registers the named HttpClient for DuckDuckGoWebSearch and centralizes transport configuration used by both the active-providers path and the empty-config fallback. This helper wires a named HttpClient with a long‑lived handler and a CookieContainer so the session established on DuckDuckGo's homepage persists into subsequent searches, reducing first‑request anomalies. It also avoids pinning a single User-Agent here, since per-request headers (including User-Agent rotation and Sec-Fetch-* context) are applied on each HttpRequestMessage by DuckDuckGoWebSearch. Endpoint URLs are absolute for this client, so a shared BaseAddress cannot be used.

## Remarks
One source of truth for the DuckDuckGo transport configuration, ensuring cookies survive across requests and keeping request headers per-request rather than globally pinned. This keeps the transport behavior consistent across the active-providers path and the empty-config fallback, and it isolates HTTP concerns from higher‑level search logic. The 1‑hour handler lifetime balances session continuity with the need to reflect DNS changes and resource management.

## Notes
- This HttpClient is tailored to DuckDuckGoWebSearch; reuse for other endpoints is discouraged since they may require different BaseAddress/timeout rules.
- Handler lifetime is one hour; if you adjust this, verify that cookie persistence and session behavior remain correct.
- Do not set DefaultRequestHeaders here; per-request headers are applied by DuckDuckGoWebSearch on each request.