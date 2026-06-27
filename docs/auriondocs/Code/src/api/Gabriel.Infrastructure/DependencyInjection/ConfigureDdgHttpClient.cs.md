Registers and configures the named HttpClient used by DuckDuckGoWebSearch. Call this from your DI setup to ensure the named client uses a short request timeout, automatic response decompression, a long-lived CookieContainer for session continuity, and an extended handler lifetime so cookies survive across related requests.

## Remarks
This central configuration ensures both the active providers path and the empty-config fallback share the same HttpClient behavior for DuckDuckGo queries. Keeping a dedicated CookieContainer on the handler preserves session cookies that DuckDuckGo sets during homepage pre-warm requests so subsequent /html/ and /lite/ searches look like continued sessions (reducing heuristics that treat requests as cold). Decompression is enabled on the handler to avoid receiving compressed bytes that can't be parsed as text, and the handler lifetime is increased to one hour (from the default two minutes) to avoid the cookie jar being replaced mid-conversation while still avoiding indefinite DNS caching.

## Example
```csharp
// In your startup/DI registration code
private void ConfigureServices(IServiceCollection services)
{
    // Other service registrations...
    ConfigureDdgHttpClient(services);
}
```

## Notes
- Do not set DefaultRequestHeaders on this named client for values that should rotate (for example User-Agent). Per-request headers are applied in DuckDuckGoWebSearch on each HttpRequestMessage to allow header rotation and correct Sec-Fetch/Referer context.
- The configured Timeout is 15 seconds; increase it if your environment needs more time for external requests.
- Handler lifetime is set to one hour to balance session persistence against DNS staleness; if your deployment relies on very frequent DNS changes, tune the lifetime accordingly.