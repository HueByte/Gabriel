# FindTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/FindTool.cs`  
> **Kind:** class

Find files by glob pattern using standard double-star recursive globs (e.g. "**/*.cs", "src/**/test_*.py"). Use this tool when you need to search a filesystem subtree from the agent's active root and want common conveniences: default noisy-directory exclusions, a configurable max-results limit (default 100, hard cap 500), and canonical path resolution via the agent's IAgentPathResolver. The tool returns a plain string; on failure it returns an error string prefixed with "Error:".

## Remarks
FindTool is a small orchestration wrapper around Microsoft.Extensions.FileSystemGlobbing.Matcher and the agent's path resolution logic. It resolves the provided root (relative roots are interpreted against the agent's active root; absolute roots must canonicalize under it) before performing a glob search, applies a default set of directory-name exclusions (node_modules, bin, obj, .git, dist, .vs, .idea, .vscode) unless explicitly overridden, and enforces a configurable maximum result count. This class centralizes filesystem search behavior so callers don't need to reimplement pattern parsing, exclusion handling, or result capping.

## Example
```csharp
// Assume 'paths' implements IAgentPathResolver and has been provided by the runtime.
var tool = new FindTool(paths);
string args = """
{
  "pattern": "**/*.cs",
  "root_path": "src",
  "mode": "host",
  "max_results": 50
}
""";
string result = await tool.ExecuteAsync(args, CancellationToken.None);
// 'result' contains the tool's output as a string; on error it begins with "Error:".
```

## Notes
- The "pattern" property is required and must be a non-empty string; the tool validates this and returns an error if missing or invalid.
- Default noisy-directory exclusions can be disabled by passing an empty array for "exclude_dirs".
- max_results defaults to 100 and is clamped by a hard cap of 500.
- If the resolved search root points to a file, ExecuteAsync returns an error stating the root is a file rather than a directory.
- The returned value is a plain string. Callers should not assume a specific structured format unless the caller inspects BuildResults implementation (it may be plain text, JSON, or another format depending on implementation details).
