# ListDirTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/ListDirTool.cs`  
> **Kind:** class

```csharp
public sealed class ListDirTool : ITool
```


List the contents of a directory (a small "ls -la" equivalent) rooted under the configured host root or the active project sandbox. Use this tool when you need a human-friendly, table-style listing of files and directories (columns: type, size, modified, name), optionally walking subdirectories. Use mode="host" (the default) to operate under AgentTools:HostRoot or mode="project" to operate inside the conversation's project sandbox.

## Remarks
This tool is a safe, bounded directory lister intended for interactive use by agents. It delegates path canonicalization and root scoping to the environment's path resolver so callers cannot escape the configured root; it also enforces an entry cap to avoid extremely large outputs (the hard cap comes from AgentToolsOptions.MaxListEntries and the default count from AgentToolsOptions.DefaultListEntries). Errors are returned as text values prefixed with "Error:" rather than being thrown, making the tool easier to call from automation that expects string results.

## Example
```csharp
// Request a recursive, project-scoped listing including hidden entries, limited to 100 entries.
string argsJson = "{ \"path\": \"./\", \"mode\": \"project\", \"recursive\": true, \"max_entries\": 100, \"include_hidden\": true }";
string result = await tool.ExecuteAsync(argsJson, CancellationToken.None);
// 'result' will contain a plain-text table (dirs first, alphabetical) or an "Error: ..." message.
```

## Notes
- The output is a human-readable plain-text table (not structured JSON); recursive entries are indented by depth and the listing is truncated when max_entries is reached.
- If the resolved path is a file, the tool returns: "Error: '<display>' is a file, not a directory. Use file_info to inspect files." — it does not attempt to show file contents.
- Supplying a large max_entries is limited by the configured hard cap (AgentToolsOptions.MaxListEntries); invalid or malformed argument JSON results in an "Error: ..." return value rather than an exception.