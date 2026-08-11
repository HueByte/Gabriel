# DocsListTool

> **File:** `src/api/Gabriel.Engine/Tools/Docs/DocsListTool.cs`  
> **Kind:** class

```csharp
public sealed class DocsListTool : ITool
```


DocsListTool retrieves Gabriel's official docs and renders a two-source, path-sorted catalog, prioritizing the LLM-native pages for quick, canonical reference. Use it when you need a complete inventory of what Gabriel's documentation covers and a direct path you can pass to docs_read to view a page.

## Remarks

It provides a single, authoritative view by grouping entries by their source (local LLM-native vs GitHub human-prose) and presenting them in path order. This design helps you quickly assess coverage and navigate to a page without manually scanning multiple sources. The tool gracefully reports errors from the underlying docs lookup and returns a clear fallback message when docs are unavailable.

## Notes

- Deterministic ordering: LocalLlmNative group comes first according to source priority.
- Empty results trigger a user-facing message suggesting fallback discovery.
- Titles are shown alongside paths when available; entries are listed in path-sorted order.