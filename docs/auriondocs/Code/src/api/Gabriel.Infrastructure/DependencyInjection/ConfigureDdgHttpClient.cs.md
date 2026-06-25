Configures and registers a named HttpClient for the DuckDuckGoWebSearch component within the dependency injection container. This method centralizes the shared HttpClient setup used by both the active-providers path and the empty-config fallback, ensuring consistent behavior across contexts. The client is configured with a 15-second timeout and a dedicated HttpMessageHandler that keeps cookies across requests, supports automatic decompression, and preserves session state from the homepage pre-warm into subsequent searches. The handler lifetime is extended to one hour to prevent cookie state from being discarded mid-conversation. Per-request headers (User-Agent rotation, Sec-Fetch-Site, Referer) are applied on each HttpRequestMessage by DuckDuckGoWebSearch; setting DefaultRequestHeaders here would pin a single User-Agent for the lifetime of the client and disable the rotation strategy.

## Remarks

Isolate HttpClientFactory configuration from higher-level code so changes to how the DDG client is created won’t ripple through other providers. This setup preserves session state across multiple requests, reducing first-request anomalies and collapsing the gap between homepage pre-warm and actual searches. It also aligns with HttpClientFactory best practices by sharing a single configuration source for both active and fallback paths while allowing per-request header customization elsewhere.

## Notes

- Long-lived HttpClientHandler lifetime means cookies and session state survive across multiple requests; ensure this behavior fits multi-tenant or long-running test scenarios.
- Endpoints are absolute; there is no single BaseAddress to share across subdomains, so requests must specify full URLs.
- Do not rely on DefaultRequestHeaders here; per-request headers (e.g., User-Agent rotation) must be applied on each request to preserve header variability.