# ReadProjectFileTool

> **File:** `src/api/Gabriel.Engine/Tools/Projects/ReadProjectFileTool.cs`  
> **Kind:** class

```csharp
public sealed class ReadProjectFileTool : ITool
```


Fetches the text contents of a file from the current project associated with this conversation. It accepts either the file's GUID (as shown by list_project_files) or its filename (case-insensitive). Only text-like files (code, Markdown, JSON, plain text, etc.) are readable; binary formats are refused to avoid dumping non-text data into the chat. The output is bounded by a default maximum of about 20,000 characters and can be increased up to 80,000 characters via the max_bytes parameter; when provided, max_bytes is clamped to the range [1024, 80000]. If a filename is supplied, the tool resolves it to a GUID by listing the project’s files and matching by name. If resolution fails or the file cannot be read, the tool returns a descriptive error string. 

## Remarks
This abstraction centralizes access to project files for conversational tooling, encapsulating filename-to-GUID resolution and text extraction while enforcing safe, predictable boundaries. It relies on the current conversation being attached to a project, and on the project’s file service to perform the read, which helps keep file access consistent across tools.

## Notes
- If multiple files share the same name (case-insensitive), the first match found is used, which may be surprising in edge cases with duplicates.
- The tool communicates failures via error strings rather than exceptions; callers should handle these as runtime errors in the dialogue flow.
- A valid project context is required; calls without an attached project will return an explicit error.