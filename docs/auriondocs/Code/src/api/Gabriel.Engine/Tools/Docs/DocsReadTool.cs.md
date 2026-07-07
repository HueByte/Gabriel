# DocsReadTool

> **File:** `src/api/Gabriel.Engine/Tools/Docs/DocsReadTool.cs`  
> **Kind:** class

```csharp
public sealed class DocsReadTool : ITool
```


Fetches an official Gabriel documentation page by path from the internal docs store and returns it wrapped with canonical authority markers so callers treat it as ground truth rather than mixing it with model or external context. Use this tool when you need a trusted, up-to-date Gabriel doc (architecture, agent loop, tools, contracts, etc.) rather than reconstructing information from memory or external sources.

## Remarks

By isolating doc retrieval behind the ITool contract, DocsReadTool centralizes access to Gabriel's authoritative docs and ensures provenance is explicit through the source tag and begin/end markers. The wrapper guarantees downstream components always receive content that can be unambiguously attributed to Gabriel's official docs, reducing hallucination risk when answering questions about the system's architecture or behavior. It also handles common error scenarios gracefully by returning helpful messages when a path is missing or a doc can't be found.

## Notes

- Content is wrapped with explicit authority markers to preserve provenance in downstream processing.
- If the requested doc path does not exist, you receive a clear error message guiding you to docs_list to discover valid paths (LLM-NATIVE or GitHub).