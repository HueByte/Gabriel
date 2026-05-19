using System.Text;
using System.Text.Json;
using Gabriel.Core.Configuration;
using Gabriel.Core.Exceptions;
using Microsoft.Extensions.Options;

namespace Gabriel.Engine.Tools.Files;

// `ls -la` equivalent, scoped to the configured host root (or active project
// sandbox in mode="project"). Sorts dirs-first then alphabetical; supports a
// shallow `recursive=true` walk capped by max_entries. Hard cap from
// AgentToolsOptions.MaxListEntries.
public sealed class ListDirTool : ITool
{
    private readonly IAgentPathResolver _paths;
    private readonly AgentToolsOptions _options;

    public ListDirTool(IAgentPathResolver paths, IOptions<AgentToolsOptions> options)
    {
        _paths = paths;
        _options = options.Value;
    }

    public string Name => "list_dir";

    public string Description =>
        "List the contents of a directory. " +
        "Returns a table-style listing (type, size, modified, name) sorted dirs-first then alphabetical. " +
        "Supports a recursive walk (entries indented by depth) capped by max_entries. " +
        "Defaults to host mode rooted at AgentTools:HostRoot. " +
        "Pass mode=\"project\" to list inside this conversation's project sandbox.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "path": {
              "type": "string",
              "description": "Directory to list. Relative resolves under the active root; absolute must canonicalize under it. Defaults to the root.",
              "default": "."
            },
            "mode": {
              "type": "string",
              "enum": ["host", "project"],
              "default": "host"
            },
            "recursive": {
              "type": "boolean",
              "default": false,
              "description": "Walk subdirectories. Output is truncated at max_entries either way."
            },
            "max_entries": {
              "type": "integer",
              "default": 200,
              "minimum": 1,
              "maximum": 1000
            },
            "include_hidden": {
              "type": "boolean",
              "default": false,
              "description": "Include dotfiles / dot-directories (Unix convention) and entries with the Hidden attribute."
            }
          },
          "required": []
        }
        """;

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        string pathArg;
        PathRootMode mode;
        bool recursive;
        int maxEntries;
        bool includeHidden;
        try
        {
            (pathArg, mode, recursive, maxEntries, includeHidden) = ParseArgs(
                argumentsJson, _options.DefaultListEntries, _options.MaxListEntries);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }

        ResolvedPath resolved;
        try
        {
            resolved = await _paths.ResolveAsync(pathArg, mode, ct);
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
                return $"Error: '{resolved.Display}' is a file, not a directory. Use file_info to inspect files.";
            return $"Error: directory not found - {resolved.Display} (looked at {resolved.Absolute}).";
        }

        return BuildListing(resolved, recursive, maxEntries, includeHidden, ct);
    }

    private static (string Path, PathRootMode Mode, bool Recursive, int MaxEntries, bool IncludeHidden) ParseArgs(
        string argumentsJson, int defaultEntries, int hardCap)
    {
        // Accept empty / "{}" args; default to listing the root.
        var path = ".";
        var mode = PathRootMode.Host;
        var recursive = false;
        var maxEntries = defaultEntries;
        var includeHidden = false;

        if (string.IsNullOrWhiteSpace(argumentsJson))
            return (path, mode, recursive, maxEntries, includeHidden);

        using var doc = JsonDocument.Parse(argumentsJson);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            return (path, mode, recursive, maxEntries, includeHidden);

        if (root.TryGetProperty("path", out var pathEl) && pathEl.ValueKind == JsonValueKind.String)
        {
            var s = pathEl.GetString();
            if (!string.IsNullOrWhiteSpace(s)) path = s;
        }

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

        if (root.TryGetProperty("recursive", out var recEl) &&
            (recEl.ValueKind == JsonValueKind.True || recEl.ValueKind == JsonValueKind.False))
        {
            recursive = recEl.GetBoolean();
        }

        if (root.TryGetProperty("max_entries", out var maxEl) && maxEl.ValueKind == JsonValueKind.Number)
        {
            maxEntries = Math.Clamp(maxEl.GetInt32(), 1, hardCap);
        }

        if (root.TryGetProperty("include_hidden", out var hidEl) &&
            (hidEl.ValueKind == JsonValueKind.True || hidEl.ValueKind == JsonValueKind.False))
        {
            includeHidden = hidEl.GetBoolean();
        }

        return (path, mode, recursive, maxEntries, includeHidden);
    }

    private static string BuildListing(ResolvedPath resolved, bool recursive, int maxEntries, bool includeHidden, CancellationToken ct)
    {
        var sb = new StringBuilder();
        sb.Append("Listing: ").Append(resolved.Display)
          .Append("   (mode: ").Append(resolved.Mode == PathRootMode.Host ? "host" : "project")
          .Append(recursive ? ", recursive" : ", shallow").AppendLine(")");
        sb.Append("Absolute: ").AppendLine(resolved.Absolute);

        var entries = new List<Entry>();
        var truncated = CollectEntries(resolved.Absolute, depth: 0, recursive, includeHidden, maxEntries, entries, ct);

        if (entries.Count == 0)
        {
            sb.AppendLine();
            sb.AppendLine("(empty directory)");
            return sb.ToString().TrimEnd();
        }

        // Sort dirs-first, alphabetical within each kind. For recursive
        // listings, sort within each parent so the tree stays grouped.
        entries.Sort((a, b) =>
        {
            var pa = Path.GetDirectoryName(a.RelativePath) ?? string.Empty;
            var pb = Path.GetDirectoryName(b.RelativePath) ?? string.Empty;
            var parentCmp = StringComparer.OrdinalIgnoreCase.Compare(pa, pb);
            if (parentCmp != 0) return parentCmp;
            if (a.IsDirectory != b.IsDirectory) return a.IsDirectory ? -1 : 1;
            return StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);
        });

        var fileCount = entries.Count(e => !e.IsDirectory);
        var dirCount = entries.Count - fileCount;

        sb.Append("Entries: ").Append(entries.Count)
          .Append(" (").Append(fileCount).Append(" file(s), ")
          .Append(dirCount).Append(" dir(s))");
        if (truncated) sb.Append(" - truncated at max_entries");
        sb.AppendLine();

        sb.AppendLine();
        sb.AppendLine("TYPE  SIZE         MODIFIED              NAME");
        sb.AppendLine("----  -----------  --------------------  ----");
        foreach (var e in entries)
        {
            ct.ThrowIfCancellationRequested();
            var type = e.IsDirectory ? "d" : (e.IsSymlink ? "l" : "-");
            var size = e.IsDirectory ? "-" : FormatSize(e.Size);
            var mtime = e.ModifiedUtc.ToString("yyyy-MM-dd HH:mm:ssZ");
            var indent = new string(' ', e.Depth * 2);
            sb.Append(type).Append("     ")
              .Append(size.PadRight(11)).Append("  ")
              .Append(mtime).Append("  ")
              .Append(indent).AppendLine(e.Name);
        }

        if (truncated)
        {
            sb.AppendLine();
            sb.Append("(... more entries omitted - refine path or use find/grep for targeted search)").AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private static bool CollectEntries(
        string absoluteDir,
        int depth,
        bool recursive,
        bool includeHidden,
        int budget,
        List<Entry> sink,
        CancellationToken ct)
    {
        FileSystemInfo[] children;
        try
        {
            children = new DirectoryInfo(absoluteDir).GetFileSystemInfos();
        }
        catch
        {
            return false;
        }
        ct.ThrowIfCancellationRequested();

        // Sort each level so depth-first traversal is deterministic.
        Array.Sort(children, (a, b) =>
        {
            var aDir = (a.Attributes & FileAttributes.Directory) != 0;
            var bDir = (b.Attributes & FileAttributes.Directory) != 0;
            if (aDir != bDir) return aDir ? -1 : 1;
            return StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);
        });

        foreach (var child in children)
        {
            if (sink.Count >= budget) return true;
            if (!includeHidden && IsHidden(child)) continue;

            var isDir = (child.Attributes & FileAttributes.Directory) != 0;
            var isLink = (child.Attributes & FileAttributes.ReparsePoint) != 0;
            var size = isDir ? 0L : (child is FileInfo fi ? fi.Length : 0L);

            sink.Add(new Entry(
                Name: child.Name,
                RelativePath: child.FullName,
                IsDirectory: isDir,
                IsSymlink: isLink,
                Size: size,
                ModifiedUtc: child.LastWriteTimeUtc,
                Depth: depth));

            if (recursive && isDir && !isLink)
            {
                if (CollectEntries(child.FullName, depth + 1, recursive, includeHidden, budget, sink, ct))
                    return true;
            }
        }

        return sink.Count >= budget;
    }

    private static bool IsHidden(FileSystemInfo info)
    {
        if ((info.Attributes & FileAttributes.Hidden) != 0) return true;
        // Unix convention: dotfile.
        return info.Name.StartsWith('.');
    }

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KiB",
        < 1024L * 1024 * 1024 => $"{bytes / (1024.0 * 1024.0):F1} MiB",
        _ => $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GiB",
    };

    private readonly record struct Entry(
        string Name,
        string RelativePath,
        bool IsDirectory,
        bool IsSymlink,
        long Size,
        DateTime ModifiedUtc,
        int Depth);
}
