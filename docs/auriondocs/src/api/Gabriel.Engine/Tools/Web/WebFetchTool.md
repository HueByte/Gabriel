# WebFetchTool

> **File:** `src/api/Gabriel.Engine/Tools/Web/WebFetchTool.cs`  
> **Kind:** class

Fetches and returns cleaned plain-text content for a public web page identified by an absolute URL. Use this tool after a web_search result looks relevant and you need the full page text; it accepts a JSON argument with a single required property "url" and returns a single string containing metadata (final URL, content-type, length, truncation flag) followed by the page text or an error message.

## Remarks
This class implements ITool as a thin wrapper around an IUrlFetcher implementation. It handles argument parsing and basic validation, invokes the fetcher asynchronously, and converts the fetch result into a standardized, human-readable string. Fetch-related failures and malformed arguments are returned as error strings instead of throwing, so callers should treat the returned string as either the successful payload or an error message. The exact cleaning, truncation behavior, and character limits are determined by the IUrlFetcher provided.

## Example
```csharp
var tool = new WebFetchTool(myUrlFetcher);
string argsJson = "{ \"url\": \"https://example.com/article\" }";
string result = await tool.ExecuteAsync(argsJson, CancellationToken.None);
Console.WriteLine(result);

// Possible output (first lines shown):
// Fetched: https://example.com/article
// Content-Type: text/html; charset=utf-8
// Length: 8123 chars
//
// The article text starts here...
```

## Notes
- The JSON argument MUST contain a top-level string property "url"; empty or missing values produce an error string.
- Errors from the fetch operation are caught and returned as strings prefixed with "Error:" rather than thrown.
- The tool displays a "truncated" note when the returned content was cut to fit size limits; exact limits and HTML-to-text cleaning are the responsibility of the IUrlFetcher implementation.
- Do not use this for product/docs-specific queries that require internal knowledge—use docs_read for Gabriel-specific documentation lookups.