# WebSearchTool

> **File:** `src/api/Gabriel.Engine/Tools/Web/WebSearchTool.cs`  
> **Kind:** class

Performs a web search via the injected IWebSearch implementation and returns a human-readable, plain-text summary of the top results (title, URL, snippet). Reach for this tool when you need current, external information or public documentation that may be newer than the system's built-in knowledge; do not use it for Gabriel-specific internal documentation (use docs_list / docs_read instead).

## Remarks
This class is an adapter that implements ITool and delegates actual lookup work to an IWebSearch instance provided at construction. It validates and parses a JSON arguments payload (schema exposed by ParametersJsonSchema), enforces a default and clamped result limit (default 5, allowed 1–10), and converts the returned WebSearchResult list into a readable numbered list. Failures from the underlying search implementation are caught and returned as error strings rather than being propagated as exceptions; the class itself is stateless aside from the readonly IWebSearch dependency and is safe to reuse.

## Example
```csharp
// Example arguments JSON expected by ExecuteAsync
var argsJson = "{ \"query\": \"openai gpt-4 technical details\", \"limit\": 3 }";
var ct = CancellationToken.None;
string output = await webSearchTool.ExecuteAsync(argsJson, ct);
Console.WriteLine(output);
```

## Notes
- The arguments must be valid JSON and include a non-empty string "query"; "limit" is optional and will be clamped to the 1–10 range.
- The method returns plain text (a formatted list) or human-readable error messages — not structured JSON. Consumers that need structured data should parse the string or call the underlying IWebSearch directly.
- Exceptions from the IWebSearch are converted into an error string ("Error: web search failed - <message>"), so calling code should inspect the returned text to detect failures.