Registers the HTTP client and URL-fetching implementation used by the web_fetch tool. This configures a named HttpClient with a 15 second timeout and common browser headers (User-Agent and Accept-Language) so that major sites do not reject requests that present an empty or scriptable-looking UA, and it registers HttpUrlFetcher as the IUrlFetcher implementation in the DI container.

## Remarks
This method centralizes web-fetch-related service registration so callers get a single, consistently configured HttpClient for all page requests and a single IUrlFetcher implementation. Redirects are allowed by the client configuration; the codebase enforces an SSRF check against the final destination URL via request hooks rather than blocking at the initial redirect target. Using the named client from IHttpClientFactory avoids socket exhaustion while still allowing a per-client configuration surface.

## Example
```csharp
// Called during application startup to ensure the web fetch stack is available
// (example placement inside ConfigureServices in Startup or Program.cs)
// DependencyInjection.AddWebFetch(services);

// After registration, consumers resolve IUrlFetcher from DI:
// var fetcher = serviceProvider.GetRequiredService<IUrlFetcher>();
```

## Notes
- The configured DefaultRequestHeaders apply to every request made from the named client; override per-request headers by using an HttpRequestMessage when necessary.
- The 15 second Timeout covers the entire request (including redirects); long downloads or slow endpoints may need a different configuration.
- HttpUrlFetcher is registered as a singleton; its lifetime and usage should be compatible with the HttpClientFactory-managed clients it depends on.