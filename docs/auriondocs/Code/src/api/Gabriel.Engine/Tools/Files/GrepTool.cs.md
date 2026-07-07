# GrepTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/GrepTool.cs`  
> **Kind:** class

```csharp
public sealed class GrepTool : ITool
```


Search file contents under a resolved root using a .NET regular expression or a literal string. GrepTool walks files that match a provided glob (default "**/*"), skips common noisy directories by default, avoids scanning binary files and very large files, and emits ripgrep-style hits in the form path:line:content. Use this tool when you need fast, repository-scoped content search with options for literal matching, case sensitivity, limited context lines, and upper bounds on the number of matches.

## Remarks
GrepTool is a file-scanning helper that integrates path resolution (via the injected IAgentPathResolver) with a regex-backed content search. It deliberately applies several safety limits — per-file and global byte caps, maximum matches, and directory excludes — so it can be run against large workspaces without overwhelming the host or returning noisy results. The ParametersJsonSchema exposes knobs for literal vs. regex matching, glob-based file selection, context lines, and match caps; these are the surface for tailoring searches while the implementation enforces hard limits.

## Notes
- Lines longer than an internal cap are truncated for display — very long single lines may be shortened.
- The tool enforces both a per-file byte cap and a global byte cap to avoid scanning huge binaries or logs; very large files may be skipped even if they match the glob.
- By default several noisy directories are excluded (node_modules, bin, obj, .git, dist, .vs, .idea, .vscode); pass an empty exclude_dirs array to disable that behavior.
- Use literal=true to search for an exact string (the pattern is escaped); otherwise the pattern is treated as a .NET regular expression.
- The ParametersJsonSchema limits context_lines to at most 5 and max_matches to at most 1000; the implementation also has a lower default for interactive use.