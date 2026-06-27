# WebFetchTool

> **File:** `src/api/Gabriel.Engine/Tools/Web/WebFetchTool.cs`  
> **Kind:** class

Fetch and read the plain-text content of a public web page by URL, returning a short header (final URL, content type, length) followed by cleaned page text. Reach for this tool when a web_search result looks relevant and you need the full page text to answer a query; do not use it for Gabriel-specific documentation lookups (use docs_read instead).

## Remarks
WebFetchTool is a thin orchestration wrapper around an IUrlFetcher implementation. It accepts a JSON argument containing a single "url" string, delegates the network and HTML-cleaning work to the fetcher, and formats the fetch result as a human-readable string. The class is sealed and immutable (the fetcher is provided at construction), and ExecuteAsync catches fetch exceptions and returns them as error messages rather than allowing exceptions to bubble to the caller.

## Example
```csharp
// Typical usage (assuming you have an IUrlFetcher implementation):
IUrlFetcher fetcher = new MyUrlFetcher();
var tool = new WebFetchTool(fetcher);

string argsJson = "{ \"url\": \"https://example.com/article\" }";
string result = await tool.ExecuteAsync(argsJson, CancellationToken.None);
Console.WriteLine(result);

// Example output (human-readable text):
// Fetched: https://example.com/article
// Content-Type: text/html; charset=utf-8
// Length: 8423 chars
//
// (cleaned page text...)
```

## Notes
- The arguments JSON must contain an absolute URL string under the "url" property; missing or non-string values produce an error message.
- Errors and failures are returned as plain text prefixed with "Error: ..." instead of structured exceptions or JSON; callers should inspect the string to detect failures.
- The returned content is plain text (HTML stripped and normalized) and may be truncated by the underlying fetcher; the tool also reports a Length and notes when truncation occurred.