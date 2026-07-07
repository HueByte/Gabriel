# DocsListTool

> **File:** `src/api/Gabriel.Engine/Tools/Docs/DocsListTool.cs`  
> **Kind:** class

```csharp
public sealed class DocsListTool : ITool
```


DocsListTool serves as a read-only explorer for Gabriel’s official internal documentation. It fetches the active docs catalog via IDocsLookup, groups entries by their source, and renders a concise, prioritized listing that highlights the primary, LLM-native pages first and places human-prose GitHub pages after. Use it when you need a quick overview of what documentation exists and to discover a specific page by path (you can pass a path to docs_read to retrieve it).

## Remarks
DocsListTool encapsulates the presentation logic for the docs catalog, decoupling it from the storage details. By ordering sources with a fixed priority (LLM-native before GitHub) and sorting entries by path, it delivers a stable, predictable listing that is easy to scan and reference. The tool also surfaces the total page count and hints on how to fetch individual pages via docs_read, making it useful both for discovery and navigation.

## Notes
- It gracefully handles errors from the underlying docs provider by returning a concise error string that includes the exception message.
- If no docs are available, it returns a user-friendly message indicating the docs source may be unreachable.
- The output's grouping and header lines encode the source and access priority, which is important for consumers that rely on the primary vs fallback distinction.