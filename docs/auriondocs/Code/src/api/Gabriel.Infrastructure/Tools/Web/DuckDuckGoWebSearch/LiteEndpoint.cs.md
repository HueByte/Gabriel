A compile-time constant containing the DuckDuckGo "lite" search endpoint URL (https://lite.duckduckgo.com/lite/). The DuckDuckGoWebSearch implementation uses this lightweight, HTML-only endpoint when constructing search requests intended for simple scraping or parsing without JavaScript.

## Remarks
Centralising the lite endpoint in a single private constant ensures all internal request builders use the same base URL and makes it easy to switch to a different DuckDuckGo interface if needed. The "lite" endpoint returns minimal HTML (no client-side rendering), which simplifies server-side parsing.

## Example
```csharp
// Construct a query URL inside the same class
var url = $"{LiteEndpoint}?q={Uri.EscapeDataString(query)}";
var html = await httpClient.GetStringAsync(url);
// parse html...
```

## Notes
- The value includes a trailing slash; avoid duplicating slashes when concatenating paths or query strings.
- This is a private const: the value is fixed at compile time. Change requires code modification and recompilation (use readonly or configuration if runtime override is needed).
- The endpoint uses HTTPS; requests should preserve secure transport when calling this URL.