# ReadProjectFileTool

> **File:** `src/api/Gabriel.Engine/Tools/Projects/ReadProjectFileTool.cs`  
> **Kind:** class

Reads a text-like file from the conversation's associated project and returns its contents (subject to a size limit). Call this tool when an assistant or agent needs the contents of a project file — for example to inspect code, markdown, JSON, or plain text — without handling raw binary files. The tool accepts either the file's GUID or its filename (case-insensitive) and enforces a default and hard maximum byte limit so large files can be fetched in pages.

## Remarks
ReadProjectFileTool is a thin wrapper around IProjectFileService that handles resolving a filename to its GUID (by listing project files when the caller provides a name), validating that the conversation is attached to a project, applying max-bytes limits, and returning user-friendly error messages instead of throwing. It is designed for safe consumption by models: only text-like files are returned, binary files are refused, and the returned size can be capped so callers can paginate through very large files.

## Example
```csharp
// Request up to 5,000 bytes of the file (by GUID)
var args = "{ \"file_id\": \"d9b8f8c3-3f7a-4c1c-9a2a-1a2b3c4d5e6f\", \"max_bytes\": 5000 }";
string result = await readProjectFileTool.ExecuteAsync(args, cancellationToken);
// result will be either the file contents (truncated to max_bytes) or an error string

// Request by filename (case-insensitive)
var argsByName = "{ \"file_id\": \"README.md\" }";
string resultByName = await readProjectFileTool.ExecuteAsync(argsByName, cancellationToken);
```

## Notes
- The tool returns human-readable error strings for problems (e.g., no project attached, missing file, read failure) rather than throwing exceptions; callers should treat returned strings as possible error responses. 
- The max_bytes value is clamped between 1,024 and 80,000 bytes; if omitted a default of ~20,000 bytes is used. 
- Supplying a filename triggers an extra list query to resolve the name to a GUID; filename matching is case-insensitive but must exactly match an existing file name.