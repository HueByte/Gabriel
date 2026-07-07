# FindTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/FindTool.cs`  
> **Kind:** class

```csharp
public sealed class FindTool : ITool
```


FindTool provides a glob-based file search that resolves a search root and returns paths matching a given pattern (e.g. **/*.cs), skipping common noisy folders by default and capping results to guard performance. It exposes a simple ITool contract: parse input, resolve the root, run the glob, and return either the matches or a human-friendly error message.

## Remarks
- It delegates path resolution to IAgentPathResolver and PathRootMode, decoupling search scope from the search logic. 
- It relies on a globbing engine compatible with Microsoft.Extensions.FileSystemGlobbing.Matcher to support patterns like ** and * and folder wildcards. 
- It enforces sensible defaults for performance and noise reduction (default max results 100, hard cap 500; exclude list includes node_modules, bin, obj, .git, dist, .vs, .idea, .vscode).

## Notes
- Errors are surfaced as descriptive strings (prefixed with Error:) instead of thrown exceptions, so callers can display them directly to users.
- If the search root is a file or is missing, the method returns a clear error message rather than performing a search.