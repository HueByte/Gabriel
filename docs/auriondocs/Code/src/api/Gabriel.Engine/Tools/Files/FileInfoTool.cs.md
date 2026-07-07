# FileInfoTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs`  
> **Kind:** class

```csharp
public sealed class FileInfoTool : ITool
```


Inspects a single file or directory and returns a short, read-only summary suitable for a quick peek before deciding to read or edit. The report includes type (file or directory), size, last-modified time, a guessed MIME/encoding, line count, and a small head/tail preview for text files; for directories it returns entry counts and a few entry names. Use this tool when you need a fast, non-mutating overview rather than the full file contents.

## Remarks
FileInfoTool is an ITool implementation that resolves the provided path via the configured IAgentPathResolver and then builds a compact human-readable report. Paths resolve under a configured root: the default "host" mode uses AgentTools:HostRoot while the "project" mode targets the conversation's project sandbox. Binary/sniffing and preview behavior are constrained by internal limits (BinarySniffBytes controls how many bytes are sampled to detect binary content, and MaxPreviewLines caps the preview to avoid huge outputs). The tool is intentionally read-only and does not require approval.

## Example
```csharp
// Example: ask the tool to inspect a path with a 6-line head/tail preview
string argsJson = "{ \"path\": \"src/Program.cs\", \"mode\": \"project\", \"preview_lines\": 6 }";
string report = await fileInfoTool.ExecuteAsync(argsJson, CancellationToken.None);
Console.WriteLine(report);
```

## Notes
- preview_lines is clamped by the tool (maximum configured by MaxPreviewLines); passing 0 disables the head/tail preview.
- Relative paths are resolved under the active root; absolute paths must canonicalize under that root or resolution fails with an "Error: ..." string. The ExecuteAsync method returns human-readable error strings (prefixed with "Error:") rather than throwing for common validation/resolution failures.
- This tool is read-only: it only inspects and summarizes filesystem state and does not modify files or directories.