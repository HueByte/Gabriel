# HtmlEndpoint

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private const string HtmlEndpoint = "https://html.duckduckgo.com/html/"
```


HtmlEndpoint is a private const string that caches the absolute URL for DuckDuckGo's primary HTML search endpoint. It guarantees that requests retrieve the rich HTML output by hitting https://html.duckduckgo.com/html/ directly, independent of HttpClient.BaseAddress, with the lite endpoint used as a separate fallback.

## Remarks
This abstraction centralizes the endpoint choice, ensuring consistent routing to the HTML service and preventing misrouting when clients configure different base addresses. By using an absolute URL, the code remains robust against base address changes and ensures the lite fallback remains functional.

## Notes
- Private, compile-time constant: cannot be changed at runtime; inlining may occur.
- If the target endpoint changes, update this constant and recompile; otherwise calls may go to wrong host.