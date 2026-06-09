# DocsReadTool

> **File:** `src/api/Gabriel.Engine/Tools/Docs/DocsReadTool.cs`  
> **Kind:** class

Reads a single official Gabriel documentation page by path and returns the page text wrapped with explicit authority markers and a source tag. Reach for this when you need the canonical, authoritative description of Gabriel internals (architecture, agent loop, tools, contracts, etc.); if you don’t know valid paths first call the companion docs_list tool.

## Remarks
This tool is a thin ITool wrapper around an IDocsLookup implementation that resolves paths against two sources (LLM-native self-docs first, then a GitHub fallback). It intentionally wraps returned pages with BEGIN/END markers and includes a source label and optional canonical URL so downstream consumers (especially models) treat the returned content as the authoritative ground truth rather than blending it with other context. Errors from the lookup are caught and returned as human-readable strings rather than being propagated as exceptions.

## Example
```csharp
// Typical use: create the tool with an IDocsLookup implementation and call ExecuteAsync
var docsLookup = new MyDocsLookup(); // implements IDocsLookup
var tool = new DocsReadTool(docsLookup);
string args = "{ \"path\": \"README.md\" }";
string result = await tool.ExecuteAsync(args, CancellationToken.None);
Console.WriteLine(result); // prints the wrapped authoritative doc or an error message
```

## Notes
- The JSON argument must include a non-empty string property "path"; otherwise the tool returns an error message rather than throwing.
- If the lookup throws, the exception message is returned as part of an error string (no exception propagation).
- Returned content is wrapped with explicit markers and includes a source tag (e.g. "local-llm-native" or "github") and Canonical URL when available; callers should parse those markers if they need structured metadata.
- Use docs_list to discover valid paths (both LLM-NATIVE and GitHub style paths) before calling this tool.