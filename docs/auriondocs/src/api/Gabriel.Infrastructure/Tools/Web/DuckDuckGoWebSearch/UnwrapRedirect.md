Decodes DuckDuckGo redirect wrappers of the form "/l/?uddg=ENCODED_URL..." and returns the original target URL. Use this when processing DuckDuckGo search result links so consumers get the real destination rather than the DDG redirect wrapper.

## Remarks
This helper extracts the value of the uddg query parameter (up to the next '&') and percent-decodes it with Uri.UnescapeDataString to recover the wrapped URL. It also handles protocol-relative URLs returned by DuckDuckGo (those starting with "//") by prefixing "https:". If the expected wrapper shape is not found or decoding fails, the method returns the input href unchanged to preserve best-effort behavior.

## Example
```csharp
// DDG wrapper -> decoded
var decoded = UnwrapRedirect("/l/?uddg=https%3A%2F%2Fexample.com%2Fpath&rut=...");
// decoded == "https://example.com/path"

// Protocol-relative -> normalized to https
var normalized = UnwrapRedirect("//example.com/path");
// normalized == "https://example.com/path"

// No wrapper -> returned as-is
var original = UnwrapRedirect("https://site.local/page");
// original == "https://site.local/page"

// Malformed encoding -> fallback to input
var fallback = UnwrapRedirect("/l/?uddg=%E0%A4%A");
// fallback == "/l/?uddg=%E0%A4%A"
```

## Notes
- The method only percent-decodes the uddg value; it does not validate or normalize the resulting URL beyond handling protocol-relative inputs.
- If multiple "uddg=" occurrences exist, the first one is used (IndexOf is applied once).
- Decoding failures are swallowed and the original href is returned to avoid throwing during link-processing.