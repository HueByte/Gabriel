Registers and configures a named HttpClient used by DuckDuckGoWebSearch. It sets a 15s request timeout, enables automatic response decompression, and attaches a long-lived HttpClientHandler with its own CookieContainer so session cookies obtained during homepage pre-warm are retained across subsequent search requests; the handler lifetime is extended to one hour to avoid losing that cookie jar during short-lived handler recycling.

## Remarks
This method centralizes the HTTP configuration that both the active-providers code path and the empty-config fallback share, ensuring a single source of truth for how the DuckDuckGo client behaves. It intentionally avoids setting DefaultRequestHeaders so per-request headers (User-Agent rotation, Sec-Fetch-Site, Referer) can be applied by DuckDuckGoWebSearch for each HttpRequestMessage. Extending the handler lifetime is a deliberate trade-off to preserve session cookies across a chat-turn that issues multiple searches while keeping the lifetime short enough to avoid long-term DNS staleness.

## Example
```csharp
// Called from ConfigureServices to register the named DuckDuckGo client
private void ConfigureServices(IServiceCollection services)
{
    // other registrations...
    ConfigureDdgHttpClient(services);
}
```

## Notes
- The CookieContainer is shared for the lifetime of the handler (here: one hour). Do not use this named client to represent isolated user sessions where cookie isolation is required.
- DefaultRequestHeaders are deliberately left unset here; applying a static User-Agent on the client would defeat any per-request rotation implemented elsewhere.
- Handler lifetime is increased from the default 2 minutes to 1 hour to preserve session cookies; if your deployment has strict DNS or network-change requirements, consider adjusting the lifetime accordingly.
