# DocsListTool

> **File:** `src/api/Gabriel.Engine/Tools/Docs/DocsListTool.cs`  
> **Kind:** class

```csharp
public sealed class DocsListTool : ITool
```


DocsListTool is a sealed utility that queries IDocsLookup to enumerate Gabriel's official internal documentation and returns a single, formatted index. It groups pages by their source (local LLM-native docs vs GitHub-hosted prose) and sorts each group by path, rendering a concise catalog that callers can pass to docs_read to fetch a specific page. The tool prioritizes LLM-native content as the primary source and uses GitHub entries only when coverage is lacking. It's intended for scenarios where an agent needs an up-to-date inventory of internal docs to drive questions about Gabriel’s architecture, agent loop, personality system, and internal APIs.

## Remarks
This abstraction centralizes doc discovery, keeping the catalog stable even as individual pages evolve. By separating discovery from retrieval, it enables both user-facing catalogs and automated workflows to reason about the full set of internal documentation. The source-grouping also makes clear when coverage comes from canonical self-docs versus human-authored prose.

## Notes
- The output is a plain-text catalog; downstream consumers may parse lines starting with "-" to extract paths.
- If listing fails, the method returns an error string rather than throwing; callers should handle gracefully and consider fallback strategies.