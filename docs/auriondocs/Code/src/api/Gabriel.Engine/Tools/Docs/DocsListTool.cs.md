# DocsListTool

> **File:** `src/api/Gabriel.Engine/Tools/Docs/DocsListTool.cs`  
> **Kind:** class

Lists every page available in Gabriel's official internal documentation and returns a human-readable catalog grouped by source. Reach for this tool when you need to discover which doc pages exist (so you can pass a specific path to docs_read); it prefers the LLM-native docs and surfaces the human-prose GitHub fallback.

## Remarks
This tool is a discovery helper used by agents or tooling to enumerate the canonical Gabriel documentation. It delegates the actual listing to an IDocsLookup implementation, then formats the results into a readable text block. Results are grouped by source with the LLM-native (`local-llm-native`) entries prioritized over the GitHub human-prose fallback, reflecting the intended consumption order when answering questions about Gabriel.

## Example
```csharp
// Typical usage inside an agent or higher-level orchestrator
var tool = new DocsListTool(docsLookup); // docsLookup implements IDocsLookup
string output = await tool.ExecuteAsync("{}", CancellationToken.None);
// output is a plain text listing; pass any listed path to docs_read to fetch the page
Console.WriteLine(output);
```

## Notes
- ExecuteAsync never throws for lookup errors: exceptions from the underlying IDocsLookup are caught and returned as an "Error: ..." string; callers should parse the result rather than expecting exceptions.
- The ParametersJsonSchema is an empty object (no parameters expected); argumentsJson is ignored by the implementation.
- Source ordering is controlled by SourcePriority: it recognizes `local-llm-native` (primary) and `github` (fallback); unknown sources are placed last.
- Thread-safety depends on the provided IDocsLookup; DocsListTool itself holds only a reference and formats results, so it has no internal mutable state.
