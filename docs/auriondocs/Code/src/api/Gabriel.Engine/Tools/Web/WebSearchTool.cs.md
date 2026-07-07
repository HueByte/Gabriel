# WebSearchTool

> **File:** `src/api/Gabriel.Engine/Tools/Web/WebSearchTool.cs`  
> **Kind:** class

```csharp
public sealed class WebSearchTool : ITool
```


WebSearchTool wraps an IWebSearch to fetch current, external information from the open web and returns the top results, including titles, URLs, and snippets. Use it when you need up-to-date public docs or factual lookups the model might not know, and reserve it for external information rather than Gabriel-internal details (where docs_list/docs_read are more appropriate).

## Remarks
WebSearchTool acts as a thin wrapper around IWebSearch, exposing a simple, JSON-driven interface for the agent to perform web lookups. It centralizes result formatting and basic input validation, making it easier to swap in alternative search providers without changing callers. The abstraction also clarifies that web-derived data is supplementary to Gabriel's official documentation.

## Example
```csharp
// Example usage
string input = "{\"query\":\"C# StringBuilder usage\", \"limit\": 4}";
string result = await webSearchTool.ExecuteAsync(input, CancellationToken.None);
```

## Notes
- limit is clamped to the range 1–10; out-of-range values are coerced.
- The \'query\' parameter is required and cannot be empty; otherwise an error is returned.
- The tool returns a plain string formatted with the results; callers needing structured data should parse the output accordingly.