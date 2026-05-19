using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Gabriel.Core.Exceptions;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Options;

namespace Gabriel.Engine.Tools.Files;

// Regex / literal content search. .NET-Regex backed - walks files matching
// `path_glob` (default `**/*`) under the resolved root, applies the noisy-dir
// excludes shared with `find`, and emits ripgrep-style `path:line:content`
// hits with optional context lines.
public sealed class GrepTool : ITool
{
    private const int LineDisplayCap = 240;
    private const int DefaultMaxMatches = 200;
    private const int HardCapMatches = 1000;
    // Hard cap on the file size we'll scan. Binary blobs and giant logs aren't
    // worth feeding through the regex engine.
    private const long PerFileByteCap = 4 * 1024 * 1024;
    // Cap on bytes scanned across the whole walk. Backstop for `path_glob=**/*`
    // pointing at a huge tree by accident.
    private const long GlobalByteCap = 256L * 1024 * 1024;

    private static readonly string[] DefaultExcludes = new[]
    {
        "node_modules", "bin", "obj", ".git", "dist", ".vs", ".idea", ".vscode",
    };

    private readonly IAgentPathResolver _paths;

    public GrepTool(IAgentPathResolver paths)
    {
        _paths = paths;
    }

    public string Name => "grep";

    public string Description =>
        "Search file contents for a regex or literal string. " +
        "Walks files matching `path_glob` (default \"**/*\") under the root and emits ripgrep-style " +
        "`path:line:content` hits with optional context lines. " +
        "Pass literal=true to escape the pattern as a literal string. " +
        "Skips binary files automatically, plus the usual noisy directories (node_modules, bin, obj, .git, dist).";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "pattern": {
              "type": "string",
              "description": "Regex pattern (or literal string if literal=true). Required."
            },
            "literal": {
              "type": "boolean",
              "default": false,
              "description": "If true, the pattern is escaped via Regex.Escape and matched literally."
            },
            "path_glob": {
              "type": "string",
              "default": "**/*",
              "description": "Glob limiting which files are scanned (e.g. \"**/*.cs\", \"src/**/*.tsx\")."
            },
            "root_path": {
              "type": "string",
              "default": ".",
              "description": "Directory to search under. Relative resolves against the active root."
            },
            "mode": {
              "type": "string",
              "enum": ["host", "project"],
              "default": "host"
            },
            "context_lines": {
              "type": "integer",
              "default": 0,
              "minimum": 0,
              "maximum": 5,
              "description": "Lines of context shown before and after each match."
            },
            "max_matches": {
              "type": "integer",
              "default": 200,
              "minimum": 1,
              "maximum": 1000
            },
            "case_sensitive": {
              "type": "boolean",
              "default": false
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
        Args args;
        try
        {
            args = ParseArgs(argumentsJson);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }

        ResolvedPath resolved;
        try
        {
            resolved = await _paths.ResolveAsync(args.RootPath, args.Mode, ct);
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

        Regex regex;
        try
        {
            var raw = args.Literal ? Regex.Escape(args.Pattern) : args.Pattern;
            var opts = RegexOptions.Compiled | RegexOptions.CultureInvariant;
            if (!args.CaseSensitive) opts |= RegexOptions.IgnoreCase;
            regex = new Regex(raw, opts, TimeSpan.FromSeconds(2));
        }
        catch (ArgumentException ex)
        {
            return $"Error: invalid regex pattern - {ex.Message}";
        }

        return Search(resolved, regex, args, ct);
    }

    private static Args ParseArgs(string argumentsJson)
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        var root = doc.RootElement;

        if (!root.TryGetProperty("pattern", out var patEl) || patEl.ValueKind != JsonValueKind.String)
            throw new DomainException("'pattern' is required and must be a string.");
        var pattern = patEl.GetString();
        if (string.IsNullOrWhiteSpace(pattern))
            throw new DomainException("'pattern' cannot be empty.");

        var literal = false;
        if (root.TryGetProperty("literal", out var litEl) &&
            (litEl.ValueKind == JsonValueKind.True || litEl.ValueKind == JsonValueKind.False))
            literal = litEl.GetBoolean();

        var pathGlob = "**/*";
        if (root.TryGetProperty("path_glob", out var pgEl) && pgEl.ValueKind == JsonValueKind.String)
        {
            var s = pgEl.GetString();
            if (!string.IsNullOrWhiteSpace(s)) pathGlob = s;
        }

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

        var ctx = 0;
        if (root.TryGetProperty("context_lines", out var cEl) && cEl.ValueKind == JsonValueKind.Number)
            ctx = Math.Clamp(cEl.GetInt32(), 0, 5);

        var maxMatches = DefaultMaxMatches;
        if (root.TryGetProperty("max_matches", out var mmEl) && mmEl.ValueKind == JsonValueKind.Number)
            maxMatches = Math.Clamp(mmEl.GetInt32(), 1, HardCapMatches);

        var caseSensitive = false;
        if (root.TryGetProperty("case_sensitive", out var csEl) &&
            (csEl.ValueKind == JsonValueKind.True || csEl.ValueKind == JsonValueKind.False))
            caseSensitive = csEl.GetBoolean();

        string[] excludes = DefaultExcludes;
        if (root.TryGetProperty("exclude_dirs", out var exEl) && exEl.ValueKind == JsonValueKind.Array)
        {
            excludes = exEl.EnumerateArray()
                .Where(e => e.ValueKind == JsonValueKind.String)
                .Select(e => e.GetString()!)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
        }

        return new Args(pattern, literal, pathGlob, rootPath, mode, ctx, maxMatches, caseSensitive, excludes);
    }

    private static string Search(ResolvedPath resolved, Regex regex, Args args, CancellationToken ct)
    {
        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
        matcher.AddInclude(args.PathGlob);
        foreach (var dir in args.ExcludeDirs)
        {
            if (string.IsNullOrWhiteSpace(dir)) continue;
            matcher.AddExclude($"**/{dir}/**");
            matcher.AddExclude($"{dir}/**");
        }

        List<string> filesToScan;
        try
        {
            var matched = matcher.Execute(new Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoWrapper(new DirectoryInfo(resolved.Absolute)));
            filesToScan = matched.Files
                .Select(f => Path.Combine(resolved.Absolute, f.Path))
                .ToList();
        }
        catch (Exception ex)
        {
            return $"Error: glob expansion failed - {ex.Message}";
        }

        var sb = new StringBuilder();
        var header = $"grep: pattern='{args.Pattern}'{(args.Literal ? " (literal)" : "")} glob='{args.PathGlob}' under '{resolved.Display}'  (mode: {(resolved.Mode == PathRootMode.Host ? "host" : "project")})";
        sb.AppendLine(header);

        var totalMatches = 0;
        var filesWithHits = 0;
        var truncated = false;
        var skippedBinary = 0;
        var skippedLarge = 0;
        var bytesScanned = 0L;

        var perFileHits = new List<(int LineNo, string Line)>();

        foreach (var absolute in filesToScan)
        {
            ct.ThrowIfCancellationRequested();
            if (totalMatches >= args.MaxMatches) { truncated = true; break; }
            if (bytesScanned >= GlobalByteCap) { truncated = true; break; }

            FileInfo fi;
            try { fi = new FileInfo(absolute); }
            catch { continue; }
            if (!fi.Exists) continue;
            if (fi.Length == 0) continue;
            if (fi.Length > PerFileByteCap) { skippedLarge++; continue; }

            byte[] bytes;
            try { bytes = File.ReadAllBytes(absolute); }
            catch { continue; }
            bytesScanned += bytes.Length;

            if (LooksBinary(bytes)) { skippedBinary++; continue; }

            // Strip a UTF-8 BOM if present.
            var start = (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) ? 3 : 0;
            var text = Encoding.UTF8.GetString(bytes, start, bytes.Length - start);
            var lines = text.Split('\n');

            perFileHits.Clear();
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Length > 0 && line[^1] == '\r') line = line[..^1];

                bool matched;
                try { matched = regex.IsMatch(line); }
                catch (RegexMatchTimeoutException) { matched = false; }
                if (!matched) continue;

                perFileHits.Add((i + 1, line));
                if (totalMatches + perFileHits.Count >= args.MaxMatches) break;
            }

            if (perFileHits.Count == 0) continue;
            filesWithHits++;

            var displayPath = Path.GetRelativePath(resolved.Absolute, absolute).Replace('\\', '/');

            for (var i = 0; i < perFileHits.Count; i++)
            {
                var (lineNo, line) = perFileHits[i];

                if (args.ContextLines > 0)
                {
                    var ctxStart = Math.Max(0, lineNo - 1 - args.ContextLines);
                    var ctxEnd = Math.Min(lines.Length - 1, lineNo - 1 + args.ContextLines);
                    for (var c = ctxStart; c <= ctxEnd; c++)
                    {
                        var marker = c == lineNo - 1 ? ':' : '-';
                        var ctxLine = lines[c];
                        if (ctxLine.Length > 0 && ctxLine[^1] == '\r') ctxLine = ctxLine[..^1];
                        sb.Append(displayPath).Append(marker).Append(c + 1).Append(marker).AppendLine(TruncateLine(ctxLine));
                    }
                    // Separator between hit blocks inside the same file, so
                    // adjacent matches with overlapping context don't blur.
                    if (i < perFileHits.Count - 1) sb.AppendLine("--");
                }
                else
                {
                    sb.Append(displayPath).Append(':').Append(lineNo).Append(':').AppendLine(TruncateLine(line));
                }
            }

            totalMatches += perFileHits.Count;
            if (totalMatches >= args.MaxMatches) { truncated = true; break; }
        }

        sb.AppendLine();
        if (totalMatches == 0)
        {
            sb.Append("(no matches");
            if (skippedBinary > 0 || skippedLarge > 0)
            {
                sb.Append("; skipped ");
                if (skippedBinary > 0) sb.Append(skippedBinary).Append(" binary");
                if (skippedBinary > 0 && skippedLarge > 0) sb.Append(", ");
                if (skippedLarge > 0) sb.Append(skippedLarge).Append(" too-large");
                sb.Append(" file(s)");
            }
            sb.AppendLine(")");
        }
        else
        {
            sb.Append("--- ").Append(totalMatches).Append(" match(es) across ")
              .Append(filesWithHits).Append(" file(s)");
            if (truncated) sb.Append(" (truncated at max_matches)");
            if (skippedBinary > 0) sb.Append(", skipped ").Append(skippedBinary).Append(" binary");
            if (skippedLarge > 0) sb.Append(", skipped ").Append(skippedLarge).Append(" too-large");
            sb.AppendLine(" ---");
        }

        return sb.ToString().TrimEnd();
    }

    private static string TruncateLine(string line)
    {
        if (line.Length <= LineDisplayCap) return line;
        return line[..LineDisplayCap] + "…";
    }

    private static bool LooksBinary(byte[] bytes)
    {
        var sniff = Math.Min(bytes.Length, 4096);
        for (var i = 0; i < sniff; i++)
            if (bytes[i] == 0) return true;
        return false;
    }

    private sealed record Args(
        string Pattern,
        bool Literal,
        string PathGlob,
        string RootPath,
        PathRootMode Mode,
        int ContextLines,
        int MaxMatches,
        bool CaseSensitive,
        string[] ExcludeDirs);
}
