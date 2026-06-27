Registers a named HttpClient (HttpUrlFetcher.HttpClientName) with sane defaults and the URL fetch implementation into the provided IServiceCollection. Use this during application startup to enable the web-fetch functionality used by the web_fetch tool: the client is given a 15-second timeout and browser-like headers (User-Agent and Accept-Language) so common sites do not reject requests; an IUrlFetcher backed by HttpUrlFetcher is also registered as a singleton.

## Remarks
This centralizes HTTP configuration for all URL fetches: a single, named HttpClient prevents socket exhaustion that can occur from creating many short-lived HttpClient instances and ensures consistent timeout and header behavior across requests. The browser-like User-Agent is deliberate — many major sites block or alter responses for empty or scriptable-looking agents. Redirects are permitted; any SSRF protection runs against the final destination via request hooks.

## Example
```csharp
// inside your application's startup/configuration
// ensure this method is invoked while building the IServiceCollection
AddWebFetch(services);

// later, resolve the fetcher
var fetcher = services.BuildServiceProvider().GetRequiredService<IUrlFetcher>();
var result = await fetcher.FetchAsync(new Uri("https://example.com"));
```

## Notes
- The HttpClient timeout is fixed at 15 seconds; requests taking longer will be cancelled.
- The configured User-Agent and Accept-Language mimic a modern Chrome browser; some sites may still treat requests differently and the UA string may need updating over time.
- A named HttpClient is registered (HttpUrlFetcher.HttpClientName); HttpUrlFetcher must obtain that client (e.g. via IHttpClientFactory.CreateClient(name) or an appropriate typed-client registration) to pick up these settings. Also ensure HttpUrlFetcher is thread-safe since it is registered as a singleton.
