# FindTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/FindTool.cs`  
> **Kind:** class

Find files by glob pattern relative to a resolved search root. Reach for this tool when you need standard double-star recursive globbing (e.g. `**/*.cs`, `src/**/test_*.py`) with sensible defaults: noisy directories are excluded automatically, results are returned relative to the search root, and the result count is bounded to avoid unbounded output.

## Remarks
FindTool is the agent-facing wrapper that combines path resolution and glob matching: it asks an IAgentPathResolver to canonicalize the requested root (respecting the tool's "mode"), then uses standard glob semantics (Microsoft.Extensions.FileSystemGlobbing) to locate files under that root. It enforces a configurable max_results with a hard cap to prevent very large result sets and applies a default list of directory names to exclude (node_modules, bin, obj, .git, dist, .vs, .idea, .vscode) unless the caller overrides them.

## Example
```csharp
// JSON payload sent to ExecuteAsync
var argsJson = JsonSerializer.Serialize(new
{
    pattern = "**/*.cs",
    root_path = "src/ProjectA",
    mode = "project",
    max_results = 50,
    exclude_dirs = new string[] { "node_modules", "bin" } // pass [] to disable defaults
});

var findTool = new FindTool(agentPathResolver);
string result = await findTool.ExecuteAsync(argsJson, CancellationToken.None);
// result is a textual response: either an error message or the search results produced by the tool
```

## Notes
- The "pattern" property is required and must be a non-empty string; the tool returns an error if missing or blank.
- By default several noisy directories are skipped; pass an empty array for exclude_dirs ([]) to disable the default excludes.
- max_results defaults to 100 and is bounded (minimum 1, maximum 500); the tool enforces an overall hard cap to avoid excessive output.
