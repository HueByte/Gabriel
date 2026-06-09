Registers a named HttpClient configured for DuckDuckGoWebSearch with options chosen to preserve session behavior and avoid common pitfalls when issuing multiple searches. The client is given a 15s request timeout, an HttpClientHandler that automatically decompresses responses, and a long-lived CookieContainer so cookies set during homepage pre-warm persist across subsequent search requests. Handler lifetime is extended to one hour to keep that cookie jar alive across a short conversation.

## Remarks
This method centralizes the HTTP client configuration used by multiple DI composition paths (for example the active-providers path and an empty-config fallback) so they share one source of truth. It deliberately avoids setting DefaultRequestHeaders here because per-request headers (User-Agent rotation, Sec-Fetch-Site, Referer) are applied at each HttpRequestMessage; pinning headers on the client would prevent that rotation. The combination of a persistent CookieContainer and a longer handler lifetime reduces DDG's first-request heuristics that trigger on cold, one-shot clients.

## Example
```csharp
// Called during application startup to register the named client
ConfigureDdgHttpClient(services);

// Elsewhere, resolve the named client from DI
var client = httpClientFactory.CreateClient(DuckDuckGoWebSearch.HttpClientName);
// Set per-request headers (User-Agent, Referer, etc.) on each HttpRequestMessage
```

## Notes
- The CookieContainer is tied to the handler instance and therefore to the handler lifetime (1 hour). Cookies set during handler lifetime will be reused for requests that use the same pooled handler.
- Avoid setting DefaultRequestHeaders on this named client; doing so would prevent per-request User-Agent rotation and other per-request header changes.
- The handler is configured with AutomaticDecompression = DecompressionMethods.All to ensure ReadAsStringAsync returns correctly decoded text when servers use content-encoding.
- The CookieContainer may be observed or mutated indirectly by concurrent requests. Do not rely on direct, synchronous inspection or mutation of the container across threads without considering concurrency effects.