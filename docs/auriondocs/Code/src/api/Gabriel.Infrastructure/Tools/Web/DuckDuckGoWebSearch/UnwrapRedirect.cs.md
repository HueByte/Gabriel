Decodes DuckDuckGo redirect wrappers and returns the real target URL when the link uses the `uddg=` parameter; if the href is not a recognized DDG wrapper it returns the original string. Also converts protocol-relative URLs (starting with "//") to an absolute https: URL.

## Remarks
This is a small normalization helper used when extracting destinations from DuckDuckGo search results. It looks for the literal substring `uddg=` (case-sensitive), extracts the following value up to the next `&` (or the end of the string), and URL-decodes that value using Uri.UnescapeDataString. Any failure to recognize the wrapper or to decode the payload causes the method to fall back to returning the original href unchanged.

## Example
```csharp
// Typical DDG wrapped link
var href = "https://duckduckgo.com/l/?uddg=https%3A%2F%2Fexample.com%2Fpath&rut=...";
var target = UnwrapRedirect(href); // "https://example.com/path"

// Protocol-relative URL returned by DDG
var rel = "//example.com/page";
var abs = UnwrapRedirect(rel); // "https://example.com/page"

// Non-wrapped URL is returned as-is
var normal = "https://site.local/";
var same = UnwrapRedirect(normal); // "https://site.local/"
```

## Notes
- The search for the marker is case-sensitive; variants like `UDDG=` will not be recognized.
- The method performs no validation or normalization beyond decoding; callers should validate or canonicalize the returned URL before using it.
- Protocol-relative URLs are assumed to be HTTPS (the method prepends `https:`). Any decoding exceptions are swallowed and cause the original href to be returned.