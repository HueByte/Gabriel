# ReadProjectFileTool

> **File:** `src/api/Gabriel.Engine/Tools/Projects/ReadProjectFileTool.cs`  
> **Kind:** class

```csharp
public sealed class ReadProjectFileTool : ITool
```


Read the contents of a file in the current project's workspace. It accepts either the file's GUID (from list_project_files) or its filename (case-insensitive); only text-like files are readable and binary content is refused, with output limited to 20,000 characters by default and up to 80,000 via max_bytes.

## Remarks
Its role is to fetch a textual representation of a project file for display or analysis within the chat context. By accepting either a GUID or a filename, it hides the details of the underlying file identity and resolves filenames via a project-wide listing when needed. It centralizes error handling for missing files, non-text content, and read failures, ensuring consistent UX across the tool suite.

## Notes
- Requires the conversation to be attached to a project; otherwise it returns an error.
- If a filename is provided, the tool performs a follow-up lookup against list_project_files to resolve to the corresponding GUID (case-insensitive match).
- The tool refuses to dump binary content; if a file is not text-like, it returns an error instead of raw bytes.