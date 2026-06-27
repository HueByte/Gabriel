# DocsReadTool

> **File:** `src/api/Gabriel.Engine/Tools/Docs/DocsReadTool.cs`  
> **Kind:** class

Reads a single canonical Gabriel internal documentation page by path and returns a plain-text, authoritative wrapper around the page contents. Use this tool when you need Gabriel-specific, ground-truth documentation (it prefers LLM-native self-docs and falls back to GitHub docs) rather than external or third-party sources; if you don't know available paths call docs_list first.

## Remarks
This class encapsulates the policy for resolving and returning Gabriel's official docs in a form that downstream components (especially LLMs) should treat as the canonical source of truth. It delegates actual retrieval to an IDocsLookup implementation, handles errors and not-found cases, and always wraps successful results with explicit begin/end markers and a source tag (local-llm-native or github) so consumers can tell which form they received.

## Example
```csharp
// Common usage: call ExecuteAsync with a JSON object specifying the path.
var tool = new DocsReadTool(docsLookup); // docsLookup implements IDocsLookup
string args = "{\"path\": \"README.md\"}";
string result = await tool.ExecuteAsync(args, CancellationToken.None);
Console.WriteLine(result); // prints the authoritative wrapper and page content
```

## Notes
- The input JSON must contain a string "path" property; missing or non-string values produce an error message returned as a string.
- If the underlying IDocsLookup throws, ExecuteAsync catches the exception and returns an error message containing the exception text; it does not propagate exceptions.
- A null result from IDocsLookup yields a not-found message prompting the caller to use docs_list to discover valid paths.
- The returned payload is plain text with explicit markers and a source tag — it is not structured JSON.