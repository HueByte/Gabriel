using System.Text;
using System.Text.Json;
using Gabriel.Core.Exceptions;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Options;

namespace Gabriel.Engine.Tools.Files;

// Glob-based filename search. Maps to `Microsoft.Extensions.FileSystemGlobbing.Matcher`
// so the model can write idiomatic patterns like `**/*.cs` or `src/**/test_*.py`.
// Skips noisy directories by default (`node_modules`, `bin`, `obj`, `.git`, `dist`).
public sealed class FindTool : ITool
{
    private const int HardCap = 500;
    private const int DefaultMaxResults = 100;

    private static readonly string[] DefaultExcludes = new[]
    {
        "node_modules", "bin", "obj", ".git", "dist", ".vs", ".idea", ".vscode",
    };

    private readonly IAgentPathResolver _paths;

    public FindTool(IAgentPathResolver paths)
    {
        _paths = paths;
    }

    public string Name => "find";

    public string Description =>
        "Find files by glob pattern (e.g. \"**/*.cs\", \"src/**/test_*.py\"). " +
        "Uses standard double-star recursive globs - `**/` matches any depth, `*` matches anything except a slash. " +
        "Returns relative paths from the search root, capped at max_results (default 100, hard cap 500). " +
        "Skips noisy directories by default (node_modules, bin, obj, .git, dist).";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "pattern": {
              "type": "string",
              "description": "Glob pattern (e.g. \"**/*.cs\", \"src/**/test_*.py\"). Required."
            },
            "root_path": {
              "type": "string",
              "default": ".",
              "description": "Directory to search under. Relative resolves against the active root; absolute must canonicalize under it."
            },
            "mode": {
              "type": "string",
              "enum": ["host", "project"],
              "default": "host"
            },
            "max_results": {
              "type": "integer",
              "default": 100,
              "minimum": 1,
              "maximum": 500
            },
            "exclude_dirs": {
              "type": "array",
              "items": { "type": "string" },
              "description": "Directory names to skip. Defaults to [node_modules, bin, obj, .git, dist, .vs, .idea, .vscode]. Pass [] to disable."
            }
          },
          "required": ["pattern"]
        }
        """;

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        string pattern;
        string rootPath;
        PathRootMode mode;
        int maxResults;
        string[] excludeDirs;
        try
        {
            (pattern, rootPath, mode, maxResults, excludeDirs) = ParseArgs(argumentsJson);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }

        ResolvedPath resolved;
        try
        {
            resolved = await _paths.ResolveAsync(rootPath, mode, ct);
        }
        catch (DomainException ex)
        {
            return $"Error: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error: could not resolve path - {ex.Message}";
        }

        if (!Directory.Exists(resolved.Absolute))
        {
            if (File.Exists(resolved.Absolute))
                return $"Error: search root '{resolved.Display}' is a file, not a directory.";
            return $"Error: search root not found - {resolved.Display}.";
        }

        return BuildResults(resolved, pattern, maxResults, excludeDirs, ct);
    }

    private static (string Pattern, string RootPath, PathRootMode Mode, int MaxResults, string[] ExcludeDirs) ParseArgs(string argumentsJson)
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        var root = doc.RootElement;

        if (!root.TryGetProperty("pattern", out var patEl) || patEl.ValueKind != JsonValueKind.String)
            throw new DomainException("'pattern' is required and must be a string.");
        var pattern = patEl.GetString();
        if (string.IsNullOrWhiteSpace(pattern))
            throw new DomainException("'pattern' cannot be empty.");

        var rootPath = ".";
        if (root.TryGetProperty("root_path", out var rpEl) && rpEl.ValueKind == JsonValueKind.String)
        {
            var s = rpEl.GetString();
            if (!string.IsNullOrWhiteSpace(s)) rootPath = s;
        }

        var mode = PathRootMode.Host;
        if (root.TryGetProperty("mode", out var modeEl) && modeEl.ValueKind == JsonValueKind.String)
        {
            var s = modeEl.GetString();
            mode = s?.ToLowerInvariant() switch
            {
                "host" or null or "" => PathRootMode.Host,
                "project" => PathRootMode.Project,
                _ => throw new DomainException($"'mode' must be 'host' or 'project' (got '{s}')."),
            };
        }

        var maxResults = DefaultMaxResults;
        if (root.TryGetProperty("max_results", out var maxEl) && maxEl.ValueKind == JsonValueKind.Number)
            maxResults = Math.Clamp(maxEl.GetInt32(), 1, HardCap);

        // Caller-supplied exclude list overrides the default outright. Pass []
        // to explicitly disable directory pruning.
        string[] excludes = DefaultExcludes;
        if (root.TryGetProperty("exclude_dirs", out var exEl) && exEl.ValueKind == JsonValueKind.Array)
        {
            excludes = exEl.EnumerateArray()
                .Where(e => e.ValueKind == JsonValueKind.String)
                .Select(e => e.GetString()!)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
        }

        return (pattern, rootPath, mode, maxResults, excludes);
    }

    private static string BuildResults(ResolvedPath resolved, string pattern, int maxResults, string[] excludeDirs, CancellationToken ct)
    {
        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
        matcher.AddInclude(pattern);

        // FileSystemGlobbing doesn't natively walk-skip directories, so we pre-
        // exclude their contents with `**` patterns. Cheaper than enumerating
        // a `node_modules` tree just to filter it back out.
        foreach (var dir in excludeDirs)
        {
            if (string.IsNullOrWhiteSpace(dir)) continue;
            matcher.AddExclude($"**/{dir}/**");
            matcher.AddExclude($"{dir}/**");
        }

        IEnumerable<string> matches;
        try
        {
            var result = matcher.Execute(new Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoWrapper(new DirectoryInfo(resolved.Absolute)));
            matches = result.Files.Select(f => f.Path);
        }
        catch (Exception ex)
        {
            return $"Error: glob search failed - {ex.Message}";
        }
        ct.ThrowIfCancellationRequested();

        // Materialize so we can both count total + truncate.
        var paths = matches.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
        ct.ThrowIfCancellationRequested();

        var sb = new StringBuilder();
        sb.Append("find: pattern='").Append(pattern).Append("' under '")
          .Append(resolved.Display).Append("'  (mode: ")
          .Append(resolved.Mode == PathRootMode.Host ? "host" : "project").AppendLine(")");

        if (paths.Count == 0)
        {
            sb.AppendLine("(no matches)");
            return sb.ToString().TrimEnd();
        }

        var shown = Math.Min(paths.Count, maxResults);
        if (paths.Count > maxResults)
            sb.Append("Found ").Append(paths.Count).Append("+ matches; showing first ").Append(maxResults).AppendLine(".");
        else
            sb.Append("Found ").Append(paths.Count).AppendLine(" match(es).");

        sb.AppendLine();
        for (var i = 0; i < shown; i++)
        {
            // FileSystemGlobbing returns paths relative to the matcher root with
            // forward slashes - perfect for display.
            sb.AppendLine(paths[i]);
        }

        return sb.ToString().TrimEnd();
    }
}
