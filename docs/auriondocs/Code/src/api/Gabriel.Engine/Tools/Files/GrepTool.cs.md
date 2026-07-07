# GrepTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/GrepTool.cs`  
> **Kind:** class

```csharp
public sealed class GrepTool : ITool
```


Search file contents under a resolved root using a regular expression or a literal string. GrepTool walks files matched by a glob, skips binary files and common noisy directories by default, and emits ripgrep-style hits (path:line:content) with optional context lines — use it when you need a safe, configurable in-process content search within the agent's workspace.

## Remarks
GrepTool implements ITool and delegates root resolution to an IAgentPathResolver (the `mode`/`root_path` parameters control how the root is resolved). It exposes a JSON parameters schema that controls pattern vs. literal matching, globbing, context lines, case sensitivity, per-run match limits and directory exclusions. The implementation purposely enforces per-file and global byte caps and a hard maximum on reported matches to avoid consuming excessive CPU or memory when scanning large trees or binary blobs.

## Example
```csharp
// Search for "TODO" in all C# files, case-insensitive with one line of context.
var grep = new GrepTool(agentPathResolver);
var args = new {
    pattern = "TODO",
    path_glob = "**/*.cs",
    context_lines = 1,
    case_sensitive = false
};
var argumentsJson = System.Text.Json.JsonSerializer.Serialize(args);
string result = await grep.ExecuteAsync(argumentsJson, CancellationToken.None);
Console.WriteLine(result); // ripgrep-style hits or an Error: ... message
```

## Notes
- The `pattern` parameter is required; set `literal=true` to match the pattern as a literal string (it will be escaped via Regex.Escape).
- Default noisy directories are skipped (node_modules, bin, obj, .git, dist, .vs, .idea, .vscode); pass an empty `exclude_dirs` array to disable these defaults.
- The tool enforces caps (per-file byte cap, global byte cap, and a hard cap on matches). Large files, binary files, or very large trees may produce fewer results than an unbounded search would.