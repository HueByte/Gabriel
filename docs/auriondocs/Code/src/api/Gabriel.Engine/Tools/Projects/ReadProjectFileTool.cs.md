# ReadProjectFileTool

> **File:** `src/api/Gabriel.Engine/Tools/Projects/ReadProjectFileTool.cs`  
> **Kind:** class

```csharp
public sealed class ReadProjectFileTool : ITool
```


ReadProjectFileTool reads the textual contents of a file from the current project's scope in this conversation. It accepts either the file's GUID (from list_project_files) or a filename (case-insensitive). It only reads text-like files (code, Markdown, JSON, plain text, etc.) and refuses binary content to avoid dumping binary data into the chat; the output is capped by default at ~20,000 characters, with an option to raise this up to 80,000 via max_bytes.

## Remarks
By centralizing the file-reading logic behind a single ITool, ReadProjectFileTool encapsulates the common concerns of project-scoped I/O: validation of the project context, resolution of the target file (by GUID or by filename), and user-friendly error reporting instead of exceptions. When a filename is provided, the tool performs an extra lookup (list_project_files) to resolve it to a GUID, trading a small extra cost for improved resilience when only the filename is on hand. This tool relies on the project context to enforce scope and on the underlying IProjectFileService to perform the actual read, keeping responsibilities clean and testable.

## Notes
- The conversation must be attached to a project; otherwise the tool returns an error.
- Reading a binary file yields a specific error to prevent dumping non-text bytes.
- If you pass a filename, expect an additional lookup step to resolve it to the file's GUID.
- max_bytes defaults to 20000 and is clamped to the range [1024, 80000].
- If the read operation fails, the error includes the underlying exception message for troubleshooting.