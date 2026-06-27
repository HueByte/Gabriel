# GrepTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/GrepTool.cs`  
> **Kind:** class

Search file contents under a resolved root using a .NET regular expression (or a literal string when literal=true). Use this tool when you need a ripgrep-style quick content search across a workspace or project from the agent environment — it walks files matching a glob, applies common noisy-directory excludes, skips binary files, and returns matches in `path:line:content` form with optional surrounding context.

## Remarks
GrepTool wraps a file-walking, pattern-matching workflow and delegates path resolution to an IAgentPathResolver (so callers can request host vs. project roots). It enforces safety limits to avoid pathological scans: per-file and global byte caps, a cap on the number of reported matches, and a maximum displayed line length. The tool accepts a JSON-encoded argument object (see ParametersJsonSchema) and returns a single string result; parsing or resolution failures are returned as an "Error: ..." string for the caller to surface.

## Example
```csharp
// Example usage: call the tool with a JSON args object and await the result.
// Note: in real code the IAgentPathResolver would be provided by the host.
var argsJson = @"{ 
  \"pattern\": ""TODO\"",
  \"path_glob\": ""**/*.cs"",
  \"context_lines\": 1,
  \"max_matches\": 100
}";
string result = await grepTool.ExecuteAsync(argsJson, CancellationToken.None);
Console.WriteLine(result);
```

## Notes
- The pattern field is required in the JSON arguments; set literal=true to treat the input as a plain string (Regex.Escape is used).
- Defaults and safety caps: default max_matches = 200, hard cap = 1000; per-file byte cap = 4 MB; global byte cap = 256 MB; displayed line content is capped (240 chars).
- Several noisy directories are excluded by default (node_modules, bin, obj, .git, dist, .vs, .idea, .vscode); pass an empty exclude_dirs array to disable these defaults.
- context_lines is limited (schema enforces 0–5). Parsing or path resolution errors are returned as an "Error: <message>" string rather than thrown exceptions.
