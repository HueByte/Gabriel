# WebSearchTool

> **File:** `src/api/Gabriel.Engine/Tools/Web/WebSearchTool.cs`  
> **Kind:** class

```csharp
public sealed class WebSearchTool : ITool
```


WebSearchTool is a sealed adapter that wires a simple web-search capability into the Gabriel tool framework. It delegates the actual search work to an IWebSearch implementation and exposes a single command, web_search, that returns a concise, human-readable list of results: each result includes the title, the URL, and an optional snippet. Use this tool when you need current, external information such as recent events or public documentation that the model cannot reliably recall. It should not be used for Gabriel-specific architectural questions or internal APIs; for those, consult Gabriel's official docs via docs_list and docs_read.

The tool accepts a JSON object with a required query string and an optional limit (1-10, default 5). It calls the underlying search provider and formats the results as a numbered list with title, URL, and optional snippet. If no results are found, it returns No results for: query. On errors, it returns a descriptive error message rather than throwing. The class is designed to be easily testable by injecting a mock IWebSearch, centralizing web-search formatting in one place.

## Remarks
WebSearchTool serves as the bridge between Gabriel's tooling surface and external information. By depending on IWebSearch, it enables swapping in different search backends or mocking for tests without changing callers. This abstraction helps prevent the model from fabricating external facts and provides a consistent, readable presentation of results.

## Notes
- The limit is clamped to the range 1–10; values outside this range are coerced to the nearest boundary.
- If the underlying search throws, the tool returns a descriptive error string instead of propagating the exception.
- Output is a plain, numbered list containing the Title, Url, and an optional Snippet; when no results exist, a specific "No results for" message is returned.