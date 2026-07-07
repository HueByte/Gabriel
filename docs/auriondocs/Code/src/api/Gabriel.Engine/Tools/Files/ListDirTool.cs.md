# ListDirTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/ListDirTool.cs`  
> **Kind:** class

```csharp
public sealed class ListDirTool : ITool
```


List the contents of a directory scoped to the configured host root or the active project sandbox. Returns a table-style, human-readable listing (type, size, modified, name), sorted with directories first and then alphabetically; supports a shallow recursive walk with entries indented by depth. Use this tool when you need a compact, agent-friendly directory listing rooted at the agent's HostRoot (default) or the conversation's project sandbox (mode="project") instead of invoking raw shell commands or inspecting individual files with file_info.

## Remarks
This tool centralizes safe, normalized directory enumeration for the agent: it delegates path resolution to IAgentPathResolver so callers can supply relative or absolute paths that are canonicalized under the configured root, and it uses AgentToolsOptions to apply sensible defaults and a hard cap on the number of entries. Errors from invalid arguments or unresolved paths are surfaced as plain text (prefixed with "Error:") rather than thrown. The listing enforces a max_entries limit (and respects the agent-wide hard cap) to avoid producing excessively large outputs.

## Notes
- Absolute paths must canonicalize under the configured root; otherwise ResolveAsync will fail and the tool returns an error message.
- If the resolved path points to a file the tool returns: "Error: '{display}' is a file, not a directory. Use file_info to inspect files." Use file_info for single-file inspection.
- The returned listing is truncated when the count reaches max_entries; AgentToolsOptions.MaxListEntries enforces an upper hard cap on what callers may request.