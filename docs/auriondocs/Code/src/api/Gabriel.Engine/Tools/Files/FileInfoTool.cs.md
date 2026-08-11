# FileInfoTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs`  
> **Kind:** class

```csharp
public sealed class FileInfoTool : ITool
```


Inspect a single file or directory and produce a compact, read-only report useful for deciding whether to open or edit the target. Use this tool when you want a quick summary (type, size, mtime, MIME/encoding guess, line count) and a small head/tail preview of text files or an entry summary for directories; it resolves paths under the configured root (AgentTools:HostRoot by default) or the conversation's project sandbox when mode="project".

## Remarks
FileInfoTool is a lightweight, read-only inspector intended as a safe preview step before performing heavier operations (read, edit, replace). It centralizes path resolution and validation (via the injected IAgentPathResolver) and enforces size/preview limits so callers can request brief previews without reading entire files. Errors from argument parsing or path resolution are returned as a simple "Error: ..." string rather than propagated exceptions, so callers should treat non-prefixed results as the successful report body.

## Example
```csharp
// Given an existing FileInfoTool instance (fileInfoTool):
var argsJson = "{\"path\":\"README.md\",\"mode\":\"project\",\"preview_lines\":6}";
string report = await fileInfoTool.ExecuteAsync(argsJson, CancellationToken.None);
Console.WriteLine(report);
```

## Notes
- The JSON arguments must include a non-empty "path" string; invalid or missing values cause a DomainException that is returned as an "Error: ..." message. 
- The "mode" property accepts only "host" or "project"; other values result in an argument error. 
- Preview line count is bounded (MaxPreviewLines = 50) and the tool respects a configured default preview lines value when none is provided. Passing 0 for preview_lines disables the preview. 
- The tool performs binary sniffing of the first 4096 bytes to decide whether a file is text or binary; expect no text preview for files detected as binary. 
- Path resolution is performed against the active root; absolute paths must canonicalize under that root or resolution will fail with an error message.