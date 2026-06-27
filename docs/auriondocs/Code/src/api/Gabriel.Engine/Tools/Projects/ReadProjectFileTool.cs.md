# ReadProjectFileTool

> **File:** `src/api/Gabriel.Engine/Tools/Projects/ReadProjectFileTool.cs`  
> **Kind:** class

```csharp
public sealed class ReadProjectFileTool : ITool
```


Reads a text-like file from the conversation's attached project and returns its contents (or a short error message). Use this tool when the assistant needs to show or inspect a code, markdown, JSON, or plain-text file that belongs to the current project; provide either the file's GUID (preferred) or its filename (case-insensitive) as the `file_id` argument.

## Remarks
This tool enforces an output size limit to avoid dumping very large files into the conversation — a default cap of ~20,000 bytes is used and a hard ceiling of 80,000 bytes is enforced. The implementation accepts a GUID for direct lookup or a filename as a fallback; resolving by filename performs an extra list query so the model can recover when it copied a name instead of the bracketed id. Binary files are detected and refused (the tool returns an explanatory error message rather than raw bytes).

## Example
```csharp
// Request by GUID, default max bytes
var args1 = "{ \"file_id\": \"d2f5a9b2-...\" }";
var result1 = await tool.ExecuteAsync(args1, CancellationToken.None);

// Request by filename and increase allowed bytes (will be clamped to 80000)
var args2 = "{ \"file_id\": \"README.md\", \"max_bytes\": 40000 }";
var result2 = await tool.ExecuteAsync(args2, CancellationToken.None);

// Result is a string: either the file contents (possibly truncated) or an error message.
```

## Notes
- If multiple files share the same name, the first case-insensitive match from the project listing is returned — duplicate names are not disambiguated.
- The effective `max_bytes` is clamped to the range [1024, 80000]; omitted means the default ~20,000 bytes is used.
- If the conversation is not attached to a project, or the underlying file service throws, the tool returns a human-readable error string instead of throwing an exception.
