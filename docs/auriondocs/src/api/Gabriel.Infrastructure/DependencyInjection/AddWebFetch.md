Registers a configured HTTP client and the URL fetcher implementation used by the web_fetch tool. Use this during dependency-injection setup when the application needs to perform outbound web page fetches that behave like a normal browser (timeout, language, and a realistic User-Agent) and to ensure the IUrlFetcher implementation is available via DI.

## Remarks
This method centralizes HTTP client configuration for all web fetching in the app: it creates a named HttpClient (used by HttpUrlFetcher) with a conservative 15-second timeout and headers that emulate a modern browser. Having a single, factory-created HttpClient avoids socket exhaustion and ensures consistent request behavior across callers. The implementation also intentionally allows redirects; any server-side SSRF guard is applied against the final destination via request hooks rather than the initial redirect target.

## Example
```csharp
// Inside your Startup/Program DI configuration
// If this method is accessible, call it to register the fetcher and client
AddWebFetch(services);

// Later, resolve the fetcher from DI
var fetcher = services.BuildServiceProvider().GetRequiredService<IUrlFetcher>();
```

## Notes
- The client timeout is fixed to 15 seconds; long-running downloads may fail unless reconfigured.
- A realistic, browser-like User-Agent is added because many sites reject blank or scriptable-looking UAs.
- Redirects are permitted; security checks (SSRF protections) are applied against the final redirected URL via request hooks.