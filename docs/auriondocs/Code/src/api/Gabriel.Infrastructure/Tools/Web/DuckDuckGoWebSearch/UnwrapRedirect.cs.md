UnwrapRedirect extracts the final destination from a DuckDuckGo redirect URL. If the input href uses the DDG wrapper /l/?uddg=ENCODED_URL&rut=..., the uddg parameter is URL-decoded to recover the real target. If the wrapper shape is unrecognized (or the uddg value can’t be decoded), the method returns the original href. When the input is protocol-relative (starts with //), it prefixes https: to produce an absolute HTTPS URL. In all cases, the method returns a string representing the best-effort final URL.

## Remarks
UnwrapRedirect centralizes the logic for normalizing external links produced by DuckDuckGo redirects, enabling downstream code to work with stable, direct URLs. It is defensive: on unexpected inputs or decoding failures, it falls back to returning the original href rather than throwing. The helper is private static, indicating it’s an internal utility used where link normalization is needed within its class.

## Example
```csharp
// Common case: unwrap a DDG redirect to the final URL
string target = UnwrapRedirect("https://duckduckgo.com/l/?uddg=https%3A%2F%2Fexample.com%2Fpage");
// target == "https://example.com/page"
```

## Notes
- Only unwraps the uddg parameter when the DDG wrapper is present; other redirect formats are left intact.
- If decoding fails (e.g., malformed percent-encoding), the original href is returned.
- Protocol-relative URLs (starting with //) are normalized to HTTPS by prefixing https:.