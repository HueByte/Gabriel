Configures a named HttpClient for the web_fetch workflow and wires an IUrlFetcher implementation to use it. This centralizes HTTP client settings (timeout, user agent, and headers) so callers don’t construct HttpClient instances directly, and it’s intended to be invoked during application startup to ensure consistent behavior across web fetch operations.

## Remarks
By using HttpClientFactory (AddHttpClient) with a named client, the code guarantees a reusable HttpClient instance tied to HttpUrlFetcher.HttpClientName. It sets a 15-second timeout and browser-like default request headers (User-Agent and Accept-Language) to improve compatibility with major sites that reject non-browser clients. It also registers HttpUrlFetcher as a singleton IUrlFetcher, so all fetchers share a single implementation and client configuration. Redirects are allowed and the SSRF guard validates the final destination via the request hooks.

## Notes
- Ensure this method runs once during startup to avoid duplicating registrations.
- If the target environment requires different behavior (e.g., longer timeouts, different UA), adjust the settings in this initializer rather than in call sites.