Decodes DuckDuckGo redirect wrappers and returns the actual destination URL when the link uses the `uddg=` parameter (e.g. /l/?uddg=ENCODED_URL&...). If the wrapper shape isn't recognized it returns the original href; it also converts protocol-relative URLs ("//example.com") into absolute HTTPS URLs by prepending "https:".

## Remarks
This helper exists to normalize links returned by DuckDuckGo search results so callers can work with the real target instead of the DuckDuckGo redirect wrapper. It looks for the literal marker `uddg=`, extracts the following value up to the next `&` (or the end of the string), and URL-decodes that segment. Any decoding failure or an unrecognized wrapper format falls back to returning the original href.

## Example
```csharp
// Input is a DuckDuckGo redirect wrapper
var wrapped = "/l/?uddg=https%3A%2F%2Fexample.com%2Fpage&rut=xyz";
var target = UnwrapRedirect(wrapped);
// target == "https://example.com/page"

// Protocol-relative URL returned as-is by DDG
var protoRelative = "//example.com/path";
var absolute = UnwrapRedirect(protoRelative);
// absolute == "https://example.com/path"
```

## Notes
- The method does not guard against null; passing null for `href` will throw a NullReferenceException when calling IndexOf.
- Any exception thrown during URL decoding is swallowed and the original `href` is returned (safe fallback but hides decoding errors).
- Only the `uddg=` pattern is recognized; other redirect wrapper shapes will not be unwrapped and will be returned unchanged.