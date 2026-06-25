UnwrapRedirect extracts the true navigation target from a DuckDuckGo redirection wrapper. When a URL contains the uddg parameter (as in the /l/?uddg=ENCODED_URL&rut=... form), it decodes the embedded URL and returns it; if the wrapper is not present, it either prefixes https: for protocol-relative URLs (//example.com/...) or returns the original href unchanged. This helper is useful anytime you normalize or follow external links that may have been wrapped by a search proxy rather than using the literal href.

## Remarks
By isolating this logic, the code centralizes the handling of DuckDuckGo's redirect wrapper, making link normalization safer wherever the application processes results from search pages. It also gracefully handles uncommon shapes of the wrapper and decoding failures by returning the original href, avoiding exceptions in the caller. This method assumes the use of a simple uddg wrapper and does not attempt to validate the decoded URL beyond decoding it.

## Example
```csharp
// Common case: a DuckDuckGo redirect URL that wraps the real target
var wrapped = "https://duckduckgo.com/l/?uddg=https%3A%2F%2Fexample.org%2Fpage%3Fq%3Dtest&rut=...";
var real = UnwrapRedirect(wrapped);
// real == "https://example.org/page?q=test"

// If there is no uddg parameter, the raw href is returned (or https: prefix is added for protocol-relative URLs)
```

## Notes
- Decoding errors are swallowed; if Uri.UnescapeDataString throws, the original href is returned.
- If href starts with "//" and there is no uddg, the URL is prefixed with https:
- Only the first occurrence of uddg is considered; subsequent occurrences are ignored.