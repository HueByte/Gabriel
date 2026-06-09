Constant storing the DuckDuckGo "lite" search endpoint URL used internally by DuckDuckGoWebSearch. Reach for this constant when the implementation needs to build requests against DuckDuckGo's lightweight (mobile/simplified) HTML interface rather than the full web UI.

## Remarks
The lite endpoint returns a minimal HTML response that is easier to parse and lighter to transfer than DuckDuckGo's full site. Centralizing the base URL as a single constant keeps request construction consistent and makes it simpler to change the target endpoint in one place if required.

## Example
```csharp
// Build a search URL by appending a query parameter. Ensure the query is encoded.
var query = Uri.EscapeDataString("open source projects");
var url = LiteEndpoint + "?q=" + query;
// Use HttpClient to fetch the result from `url`.
```

## Notes
- The constant includes a trailing slash ("/"), so append paths or query strings carefully to avoid double slashes.
- The value is private and immutable; it is intended for internal use within the containing class.
- Always URL-encode query terms before concatenation to avoid malformed requests.
