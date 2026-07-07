# ListDirTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/ListDirTool.cs`  
> **Kind:** class

```csharp
public sealed class ListDirTool : ITool
```


List the contents of a directory under the agent's configured root with an "ls -la" style, table-formatted output. Use this tool when you need a sandbox-aware, caps-enforced directory listing (host-root vs. conversation project sandbox) rather than calling Directory APIs directly; it enforces the configured root, sorts directories first then alphabetically, and truncates output to a safe maximum.

## Remarks
ListDirTool is a small, safe wrapper around filesystem enumeration that integrates with the agent's path resolution and options. It resolves paths via IAgentPathResolver (so absolute or relative paths are validated against the active root), applies AgentToolsOptions defaults and a hard cap for max entries, and returns a human-readable table (type, size, modified, name). ExecuteAsync returns error strings for common failure cases (argument parsing, path resolution, not-a-directory), making it suitable for use in tool-driven agent flows where textual results are expected.

## Example
```csharp
// Example: list the project sandbox recursively, including hidden entries
var argsJson = """
{
  "path": "src",
  "mode": "project",
  "recursive": true,
  "max_entries": 100,
  "include_hidden": true
}
""";
var result = await listDirTool.ExecuteAsync(argsJson, CancellationToken.None);
Console.WriteLine(result);
```

## Notes
- include_hidden defaults to false; dotfiles (Unix) and Windows Hidden-attribute files are omitted unless you set include_hidden to true.
- The effective max_entries is clamped by the tool's configured hard cap (AgentToolsOptions.MaxListEntries); requesting a larger value will be reduced to that cap.
- If the resolved path exists but is a file, ExecuteAsync returns an error message directing you to use file_info instead rather than throwing an exception.
