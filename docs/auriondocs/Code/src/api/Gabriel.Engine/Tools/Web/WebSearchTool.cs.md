# WebSearchTool

> **File:** `src/api/Gabriel.Engine/Tools/Web/WebSearchTool.cs`  
> **Kind:** class

```csharp
public sealed class WebSearchTool : ITool
```


WebSearchTool is a sealed class that implements ITool to perform web searches through an injected IWebSearch backend. It exposes a tool named ``web_search`` that accepts a JSON payload with a required ``query`` and an optional ``limit`` (1–10), and returns a human-friendly list of the top results including title, URL, and an optional snippet. Use this tool when you need current, external information or third-party documentation that the model cannot reliably recall, such as recent events or public docs of external tools; do not rely on it for Gabriel-specific architecture or internal APIs.

## Remarks
WebSearchTool decouples the search provider from the tool orchestration by depending on an IWebSearch backend, enabling swapping implementations or inserting mocks for tests without changing the consumer logic. It centralizes input validation and output formatting: the input is validated and the limit is clamped to the range 1–10 (defaulting to 5), and results are presented in a stable, readable text format. Exceptions from the underlying search are captured and surfaced as descriptive error messages rather than thrown, which helps callers maintain a consistent interaction model. The class being sealed reinforces its role as a concrete, final tool in the system, with a clear responsibility to perform web lookups and present them in a predictable way.

## Notes
- If the query is missing or not a string, ExecuteAsync returns a clear error message instead of throwing.
- The limit defaults to 5 and is clamped to the inclusive range [1, 10].
- If the search yields no results, the method returns "No results for: {query}"; if an exception occurs, it returns an error string like "Error: web search failed - {ex.Message}".
