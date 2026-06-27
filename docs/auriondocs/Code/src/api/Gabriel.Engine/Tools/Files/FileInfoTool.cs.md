# FileInfoTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs`  
> **Kind:** class

Inspect a single file or directory and produce a compact, read-only summary useful for quickly deciding whether to read or edit the target. Use this tool when you need metadata (type, size, last-modified time, mime/encoding guess, line count) and a small head/tail preview for text files, or a short directory entry summary — for a lightweight peek without opening or modifying the file.

## Remarks
FileInfoTool is a small, read-only inspector intended as a fast pre-check before more expensive file operations. It resolves paths under a configured root (AgentTools:HostRoot) by default or the conversation's project sandbox when mode="project". The tool validates and canonicalizes the requested path, performs a quick binary/text sniff (BinarySniffBytes) and line-count/preview sampling, and returns a textual report. Because it returns a string (including error messages prefixed with "Error:"), callers should parse or display the text rather than expecting structured objects.

## Example
```csharp
// JSON arguments to inspect a file in the project sandbox with a 10-line head/tail preview
var argsJson = "{ \"path\": \"src/Program.cs\", \"mode\": \"project\", \"preview_lines\": 10 }";
var result = await fileInfoTool.ExecuteAsync(argsJson, CancellationToken.None);
if (result.StartsWith("Error:"))
{
    Console.WriteLine("Failed to inspect file: " + result);
}
else
{
    Console.WriteLine("Preview:\n" + result);
}
```

## Notes
- preview_lines accepts 0–50 (default is 6); 0 disables text preview entirely. The tool also caps previews using an internal MaxPreviewLines constant.
- The tool performs a quick binary/text sniff (uses a fixed byte window) and will not produce a text preview for detected binary files.
- The provided path must resolve under the active root; relative paths are resolved under that root and absolute paths must canonicalize inside it, otherwise path resolution fails and an "Error:" string is returned.
- ExecuteAsync returns plain text; callers should treat the return value as user-facing output rather than a structured API response.
- FileInfoTool is read-only and holds only readonly dependencies, making it safe for concurrent use by multiple callers.