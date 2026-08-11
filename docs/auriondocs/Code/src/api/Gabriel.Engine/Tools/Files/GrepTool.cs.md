# GrepTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/GrepTool.cs`  
> **Kind:** class

```csharp
public sealed class GrepTool : ITool
```


Search file contents under a resolved root for a regular expression or a literal string and emit ripgrep-style hits (path:line:content). Use this tool when you need a fast, file-walking content search that applies noisy-directory excludes, optional context lines, and either regex or literal matching instead of manually reading and filtering files.

## Remarks
GrepTool implements the ITool contract to expose a file-content search capability to the tool runner. It resolves the search root via IAgentPathResolver, walks files constrained by a glob, and applies a .NET regex (or an escaped literal) to each scanned line. The implementation intentionally avoids scanning large or binary blobs (PerFileByteCap and GlobalByteCap) and skips common noisy directories by default to keep searches fast and to avoid feeding irrelevant or huge files into the regex engine.

## Example
```csharp
// `paths` is an IAgentPathResolver provided by the caller environment.
var grep = new GrepTool(paths);
string argsJson = "{" +
    "\"pattern\": \"TODO\", " +
    "\"path_glob\": \"**/*.cs\", " +
    "\"context_lines\": 1" +
    "}";
string result = await grep.ExecuteAsync(argsJson, CancellationToken.None);
// `result` contains ripgrep-style lines like: "src/Foo.cs:42:        // TODO: fix this"
```

## Notes
- Default noisy directories (node_modules, bin, obj, .git, dist, .vs, .idea, .vscode) are excluded; pass an empty array for exclude_dirs to disable that behavior.
- The tool enforces safety caps: individual files above PerFileByteCap are not scanned and a GlobalByteCap limits total bytes scanned; there is also a hard cap on max_matches (HardCapMatches) even if max_matches is set higher.
- context_lines is limited (0–5) and long matched lines may be truncated for display (LineDisplayCap), so very long single-line matches may be shortened in the output.