# FileInfoTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs`  
> **Kind:** class

```csharp
public sealed class FileInfoTool : ITool
```


Inspect a single file or directory and produce a concise, read-only report — type, size, last-modified time, a MIME/encoding guess, line count, and a small head+tail preview for text files (or a brief directory listing). Use this tool when you want a fast, safe peek at a path to decide whether to read or edit it; it does not modify the filesystem and does not require approval. By default it resolves paths under the configured AgentTools:HostRoot; pass mode="project" to resolve inside the conversation's project sandbox.

## Remarks
This tool exists as a safe pre-read inspector: it centralizes path canonicalization and root confinement via IAgentPathResolver, enforces preview and listing limits from AgentToolsOptions, and performs a small binary/text sniff so callers get a meaningful preview only for text files. Because it returns a textual report rather than raw file contents, it’s intended to help decisions (open, edit, or skip) without exposing large data or allowing writes.

## Notes
- The arguments JSON must include a non-empty string "path"; invalid or missing values produce an Error string (e.g. "Error: 'path' is required and must be a string.").
- Allowed mode values are "host" (default) or "project"; absolute paths must canonicalize under the selected root, otherwise resolution fails with an error.
- preview_lines is limited (0–50). Setting preview_lines to 0 disables the head/tail preview.
- The implementation sniff-checks the first 4096 bytes to decide if a file is binary; binary files will not get a text head/tail preview.
- Limits such as MaxPreviewBytes and MaxListEntries are governed by AgentToolsOptions; large files and long listings are truncated according to those settings.
- Errors during argument parsing or path resolution are returned as strings prefixed with "Error:" rather than being thrown to the caller.