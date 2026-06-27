# ListDirTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/ListDirTool.cs`  
> **Kind:** class

List the contents of a directory rooted in the configured agent host root (or the conversation project sandbox when mode="project"). Choose this tool when you need a human-friendly, table-style directory listing (type, size, modified, name) that sorts directories first and supports a shallow recursive walk with indentation and an enforced maximum number of entries.

## Remarks
This tool is a safe, opinionated wrapper around filesystem enumeration that enforces agent-level policy (default and hard-capped listing sizes) and resolves paths via IAgentPathResolver so listings are constrained to the configured root or project sandbox. Instead of throwing for common validation problems it returns error strings (prefixed with "Error:"), making it convenient for callers that want a textual result for UI or logging.

## Example
```csharp
// List the current root (defaults)
var args = "{}";
var result = await listDirTool.ExecuteAsync(args, CancellationToken.None);

// List recursively up to 500 entries and include hidden files
var argsJson = "{ \"path\": \"./src\", \"mode\": \"host\", \"recursive\": true, \"max_entries\": 500, \"include_hidden\": true }";
var result2 = await listDirTool.ExecuteAsync(argsJson, CancellationToken.None);

// result/result2 are textual table-style listings or an error message string starting with "Error:".
```

## Notes
- If the resolved target is a file the tool returns an error suggesting use of file_info rather than throwing an exception.
- Recursive output and non-recursive output are both truncated when the total number of entries reaches max_entries; the hard cap from AgentToolsOptions.MaxListEntries is enforced.
- include_hidden controls whether dotfiles (Unix convention) and entries with the Hidden attribute are shown; it defaults to false.
- Path resolution happens via IAgentPathResolver; relative paths are resolved under the active root and absolute paths must canonicalize under that root.
- Common validation errors and parsing issues are caught and returned as textual errors (e.g. malformed JSON arguments or out-of-range max_entries).