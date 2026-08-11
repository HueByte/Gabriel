# DocsReadTool

> **File:** `src/api/Gabriel.Engine/Tools/Docs/DocsReadTool.cs`  
> **Kind:** class

```csharp
public sealed class DocsReadTool : ITool
```


DocsReadTool retrieves an official Gabriel documentation page by a given path and returns the content wrapped in explicit authority markers so downstream consumers treat it as canonical Gabriel knowledge. It uses an injected IDocsLookup to read the page and provides clear, self-describing errors when the input is invalid or the page cannot be found, guiding developers to use docs_list to discover available paths.

## Remarks
This symbol exists to centralize access to Gabriel's official documentation and to protect the returned content from being blended with auxiliary context. By wrapping the content with explicit "BEGIN/END" markers and including a source tag (e.g., local-llm-native or github), it creates a trustworthy, ground-truth snippet that downstream responses can rely on when explaining architecture, agent loop, or internal APIs. The tool deliberately defers path discovery to docs_list, keeping the retrieval path explicit and auditable, and it surfaces errors that encourage developers to verify available documentation before attempting reads.

## Notes
- Ensure you call docs_list to discover valid document paths before attempting a read; this tool does not enumerate available docs itself.
- The returned payload is annotated with origin information and is intended to be treated as ground truth about Gabriel; avoid blending it with unrelated context.
