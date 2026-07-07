# DocsReadTool

> **File:** `src/api/Gabriel.Engine/Tools/Docs/DocsReadTool.cs`  
> **Kind:** class

```csharp
public sealed class DocsReadTool : ITool
```


DocsReadTool is a dedicated retrieval tool that fetches a single official Gabriel document by its path. It first resolves the requested path against the self-hosted LLM-native documentation and falls back to the GitHub-hosted Gabriel.Engine docs, returning the content wrapped with explicit authority markers to preserve its status as ground truth. Developers reach for it when they need a canonical answer about Gabriel's architecture, agent loop, or internal APIs without risking mixing in external sources; pass the desired path via JSON and rely on the tool to surface a single, authoritative page.

## Remarks
By isolating doc reads behind a specialized ITool, this abstraction enforces a single canonical source of truth for Gabriel docs within the agent's reasoning. The wrapping markers (BEGIN OFFICIAL GABRIEL DOC: path (source: X)) ensure downstream components treat the returned text as authoritative and not blend with surrounding context. It also centralizes how documentation is fetched, so any changes to path resolution or source preference are localized to the tool implementation. The content carries a source tag (local-llm-native or github) to inform consumers about the provenance of the material.

## Notes
- The input is a JSON with a 'path' string; missing or non-string yields an error message.
- The response includes explicit BEGIN/END marker blocks; account for them when displaying to end users.
- If the doc cannot be found, the response is a plain not-found message.