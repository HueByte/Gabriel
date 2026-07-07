# FindTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/FindTool.cs`  
> **Kind:** class

```csharp
public sealed class FindTool : ITool
```


FindTool is a glob-based file search utility that locates files using patterns such as "**/*.cs" or "src/**/test_*.py", leveraging Microsoft.Extensions.FileSystemGlobbing.Matcher under the hood. It returns paths relative to a resolved search root and intentionally skips noisy directories (node_modules, bin, obj, .git, dist, and a few common IDE folders) to keep results relevant and performant. The tool enforces a max_results limit (default 100, with a hard cap of 500) to prevent unbounded scans, and it relies on an IAgentPathResolver to determine the actual root from a given path and mode. Use FindTool when you need pattern-driven discovery of files within a project or workspace instead of enumerating every file directly. 

## Remarks
FindTool encapsulates the complexity of glob interpretation and path resolution behind a simple interface. By centralizing default exclusions and result capping, it provides predictable, repeatable searches across different parts of the system and helps maintain responsive tooling when scanning large codebases. It also clearly separates concerns: path resolution, glob matching, and result formatting are orchestrated together but individually testable, making it easier to reason about search behavior in isolation from the caller.

## Notes
- Default exclusions (e.g., node_modules, bin, obj, .git, dist) may hide files inside those directories unless you explicitly override exclude_dirs. 
- The max_results is capped at 500; queries that would exceed this will be truncated to the limit, which can affect visibility of additional matches in large projects. 
- Errors and status information are surfaced as string messages (e.g., when the root is missing, is a file, or the pattern parses incorrectly), so callers should handle these as user-friendly error text.