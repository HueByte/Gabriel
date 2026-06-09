# FileInfoTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs`  
> **Kind:** class

Inspect a single file or directory and produce a compact, read-only summary useful for deciding whether to read or edit the target. The tool reports the path's type (file/directory), size, last-modified time, a guessed MIME/encoding, line count for text files, and a small head+tail preview for text files; for directories it returns entry counts and the first few entries. It resolves relative paths under the active root (AgentTools:HostRoot by default) or under the conversation's project sandbox when mode="project".

## Remarks
FileInfoTool is a lightweight, read-only probe used by agent tooling to peek at filesystem targets without performing edits or requiring approval. It delegates path resolution to IAgentPathResolver and reads only the minimum required data (sniffing a bounded number of bytes and limiting preview lines) so it can safely summarize large files or directories. Errors during argument parsing or path resolution are returned as error strings instead of thrown exceptions, and the operation honors the provided CancellationToken.

## Example
```csharp
// Build JSON arguments and call the tool. This returns a Task<string> containing
// the human-readable report or an "Error: ..." message.
string argsJson = JsonSerializer.Serialize(new {
    path = "src/Program.cs",
    mode = "host",           // or "project"
    preview_lines = 6         // 0 to disable preview
});

var tool = new FileInfoTool(pathResolver, Options.Create(new AgentToolsOptions { DefaultPreviewLines = 6 }));
string report = await tool.ExecuteAsync(argsJson, CancellationToken.None);
Console.WriteLine(report);
```

## Notes
- The JSON `path` property is required and must be a non-empty string; relative paths are resolved under the active root.  
- `preview_lines` is clamped (0 disables preview; the tool enforces an upper bound — previews are intentionally limited).  
- The tool only reads a bounded prefix (binary-sniff limit) to decide if a file is text vs. binary and to produce previews; binary files will not have text previews and large files are not fully loaded.