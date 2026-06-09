Absolute URL for DuckDuckGo's primary rich HTML search endpoint (https://html.duckduckgo.com/html/). Reach for this constant when constructing requests that must unambiguously target the rich HTML DDG host regardless of any HttpClient.BaseAddress; do not substitute it when you need the lightweight "lite" endpoint.

## Remarks
This constant exists because DuckDuckGo exposes two distinct search endpoints on different subdomains (rich HTML vs. lite HTML). Using an absolute URL prevents accidental routing to the wrong host if the named HttpClient has a BaseAddress configured. Historically, pointing both endpoints at the same host caused the lite fallback to break silently, so the code intentionally uses separate absolute endpoints.

## Example
```csharp
// Build a GET request that explicitly targets the rich HTML endpoint
var request = new HttpRequestMessage(HttpMethod.Get, HtmlEndpoint + "?q=example+query");
var response = await httpClient.SendAsync(request);
```

## Notes
- The value includes a trailing slash — take care when concatenating paths or query strings to avoid duplicate or missing separators.
- This is the primary (rich HTML) endpoint; the lightweight fallback uses a different constant (lite.duckduckgo.com).
- It's a private compile-time constant: changing it requires recompilation of the assembly.
