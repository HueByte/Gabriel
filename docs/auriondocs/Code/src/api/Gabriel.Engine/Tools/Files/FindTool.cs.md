# FindTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/FindTool.cs`  
> **Kind:** class

```csharp
public sealed class FindTool : ITool
```


FindTool is a glob-based filename search utility that maps glob patterns to Microsoft.Extensions.FileSystemGlobbing.Matcher, enabling idiomatic patterns like "**/*.cs" or "src/**/test_*.py". It skips noisy directories by default (node_modules, bin, obj, .git, dist) to keep results focused and supports limiting results with a hard cap (500) and a default max of 100 results. The tool resolves a configured search root via IAgentPathResolver, validates that root, and returns matching paths relative to that root, up to the specified max_results.

## Remarks
FindTool encapsulates globbing behind a small tool API and decouples path resolution from pattern matching. It relies on IAgentPathResolver to determine the search root and uses a centralized set of default excludes to keep results fast and relevant. The max-results cap provides predictable response sizes and protects against enumerating huge sets of files.

## Notes
- Hard cap: the implementation enforces a ceiling of 500 results; although the schema allows up to 500, actual results are bounded to 500.
- Input validation: the JSON argument must include a non-empty string 'pattern'; invalid or missing inputs produce an error message (e.g. "Error: ...").
- Path validation: if the resolved root is a file or does not exist, the tool returns a clear error indicating the root location.
