# GrepTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/GrepTool.cs`  
> **Kind:** class

Search file contents for a regular expression or a literal string and return ripgrep-style hits. Use this tool when you need a quick, repository-rooted content search from the engine (supports both regex and literal modes, configurable path globs, context lines, and per-search limits). It walks files under the resolved root, applies the default noisy-directory excludes (node_modules, bin, obj, .git, dist, .vs, .idea, .vscode) unless overridden, skips binary files, and formats matches as `path:line:content`.

## Remarks
GrepTool is a lightweight, safety-first content search utility used by the engine to inspect workspace files without spawning external processes. It intentionally enforces several caps (per-file byte cap, global byte cap, and match count caps) to avoid consuming excessive CPU or memory when run against large trees or binary blobs. The tool relies on an IAgentPathResolver to resolve the root path according to the requested mode (host or project) so callers get consistent resolution semantics across engine tools.

## Example
```csharp
// Example arguments JSON passed to ExecuteAsync
string argsJson = JsonSerializer.Serialize(new {
    pattern = "TODO",             // required
    literal = false,               // treat pattern as regex (false by default)
    path_glob = "**/*.cs",       // only scan C# files
    root_path = ".",             // search under the active root
    mode = "host",
    context_lines = 1,
    max_matches = 100,
    case_sensitive = false,
    exclude_dirs = new string[] { }
});

// Call (assuming 'grep' is an instance of GrepTool)
string result = await grep.ExecuteAsync(argsJson, CancellationToken.None);

// Typical output lines (ripgrep-style):
// src/MyFile.cs:42:            // TODO: implement feature X
// Context lines (if requested) are emitted around each match.
```

## Notes
- Default behavior is case-insensitive (case_sensitive = false). Set case_sensitive = true to perform case-sensitive matches.
- Use literal = true to treat the provided pattern as a literal string; the tool will escape it before matching.
- There are enforced limits to protect resources: a per-file byte cap (~4 MB), a global byte cap (~256 MB), a default max_matches (200) and a hard cap (1000). Requests exceeding those limits will be constrained.
- Pass an empty array for exclude_dirs to disable the default directory exclusions; otherwise the built-in noisy directories are skipped automatically.
- Context lines are limited to a small range (0–5) to avoid large outputs.
