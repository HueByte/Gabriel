Configures the dependency injection container to provide a browser-like HTTP client for the web_fetch tool. It wires a named HttpClient with a 15-second timeout and standard browser headers, and registers HttpUrlFetcher as the IUrlFetcher implementation. Redirects are allowed; the SSRF guard evaluates the final destination through request hooks. Use this during startup whenever you need production-like web-page retrieval via the web_fetch mechanism with consistent HTTP behavior.

## Remarks
This abstraction localizes all HTTP client configuration for web-page fetching. By naming the client and binding it to IUrlFetcher, the rest of the system can perform URL fetches without duplicating header or timeout logic, ensuring policy (like realistic User-Agent and SSRF safeguards) is applied uniformly.

## Notes
- Hard-coded headers (User-Agent and Accept-Language) may not suit every target; consider parameterizing or exposing a configuration switch if you encounter compatibility issues.
- If you need alternate fetch behavior (different timeout, headers, or redirects), prefer a separate named HttpClient or an override at call sites rather than modifying this client.