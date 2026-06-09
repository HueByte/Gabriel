# WebSearchTool

> **File:** `src/api/Gabriel.Engine/Tools/Web/WebSearchTool.cs`  
> **Kind:** class

Wraps an IWebSearch implementation and exposes it as an ITool that performs web searches. Use this when you need a toolable web-search capability that accepts a JSON arguments object (with a required "query" and optional "limit") and returns a human-readable summary of the top results — suitable for answering questions about current events or external public documentation.

## Remarks
Acts as an adapter between the engine's tool interface and an IWebSearch service: it parses and validates the JSON arguments, clamps/falls back the requested result limit, invokes the underlying search asynchronously, and formats the results into a numbered, human-readable list (title, URL, optional snippet). Errors and empty-result conditions are converted into readable messages rather than throwing.

## Example
```csharp
// 'search' implements IWebSearch and is provided by your application.
var tool = new WebSearchTool(search);
string argsJson = "{\"query\": \"open source LLM libraries 2025\", \"limit\": 3}";
string output = await tool.ExecuteAsync(argsJson, CancellationToken.None);
Console.WriteLine(output);
```

## Notes
- The ExecuteAsync input is a JSON string; it requires a string property "query". Missing or empty queries produce an error message.
- The "limit" value is optional, defaults to 5, and is clamped to the 1–10 range.
- The method returns plain text: successful results and error conditions are both returned as human-readable strings (not structured JSON). The exact behavior for failures depends on the injected IWebSearch implementation; any exception is converted to an "Error: web search failed - <message>" string.