using System.Text;
using System.Text.Json;
using Gabriel.Core.Configuration;
using Gabriel.Core.Exceptions;
using Microsoft.Extensions.Options;

namespace Gabriel.Engine.Tools.Files;

// Quick stat on a single path. Returns type/size/mtime/mime + a small head+tail
// preview for text files, or an entry summary for directories. Read-only, no
// approval needed. Resolves under AgentTools:HostRoot by default, or the
// current project's on-disk dir when mode="project".
public sealed class FileInfoTool : ITool
{
    private const int BinarySniffBytes = 4096;
    private const int MaxPreviewLines = 50;

    private readonly IAgentPathResolver _paths;
    private readonly AgentToolsOptions _options;

    public FileInfoTool(IAgentPathResolver paths, IOptions<AgentToolsOptions> options)
    {
        _paths = paths;
        _options = options.Value;
    }

    public string Name => "file_info";

    public string Description =>
        "Inspect a single file or directory. " +
        "Returns its type, size, last-modified time, mime guess, encoding, line count, " +
        "and a small head/tail preview for text files. " +
        "For directories, returns entry counts and the first few entries. " +
        "Use this to peek before deciding whether to read or edit. " +
        "Defaults to host-filesystem mode rooted at the configured AgentTools:HostRoot. " +
        "Pass mode=\"project\" to inspect a file inside THIS conversation's project sandbox instead.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "path": {
              "type": "string",
              "description": "Path to inspect. Relative resolves under the active root; absolute must canonicalize under it."
            },
            "mode": {
              "type": "string",
              "enum": ["host", "project"],
              "default": "host",
              "description": "host = AgentTools:HostRoot. project = this conversation's project files dir."
            },
            "preview_lines": {
              "type": "integer",
              "default": 6,
              "minimum": 0,
              "maximum": 50,
              "description": "Lines to show from the head and tail of a text file. 0 disables preview."
            }
          },
          "required": ["path"]
        }
        """;

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        string pathArg;
        PathRootMode mode;
        int previewLines;
        try
        {
            (pathArg, mode, previewLines) = ParseArgs(argumentsJson, _options.DefaultPreviewLines);
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

        return BuildReport(resolved, previewLines, ct);
    }

    private static (string Path, PathRootMode Mode, int PreviewLines) ParseArgs(string argumentsJson, int defaultPreviewLines)
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        var root = doc.RootElement;

        if (!root.TryGetProperty("path", out var pathEl) || pathEl.ValueKind != JsonValueKind.String)
            throw new DomainException("'path' is required and must be a string.");
        var path = pathEl.GetString();
        if (string.IsNullOrWhiteSpace(path))
            throw new DomainException("'path' cannot be empty.");

        var mode = PathRootMode.Host;
        if (root.TryGetProperty("mode", out var modeEl) && modeEl.ValueKind == JsonValueKind.String)
        {
            var modeStr = modeEl.GetString();
            mode = modeStr?.ToLowerInvariant() switch
            {
                "host" or null or "" => PathRootMode.Host,
                "project" => PathRootMode.Project,
                _ => throw new DomainException($"'mode' must be 'host' or 'project' (got '{modeStr}')."),
            };
        }

        var previewLines = defaultPreviewLines;
        if (root.TryGetProperty("preview_lines", out var prevEl) && prevEl.ValueKind == JsonValueKind.Number)
        {
            previewLines = Math.Clamp(prevEl.GetInt32(), 0, MaxPreviewLines);
        }

        return (path, mode, previewLines);
    }

    private string BuildReport(ResolvedPath resolved, int previewLines, CancellationToken ct)
    {
        var sb = new StringBuilder();
        sb.Append("Path: ").Append(resolved.Display)
          .Append("   (mode: ").Append(resolved.Mode == PathRootMode.Host ? "host" : "project").AppendLine(")");
        sb.Append("Absolute: ").AppendLine(resolved.Absolute);
        sb.Append("Root: ").AppendLine(resolved.RootAbsolute);

        var exists = File.Exists(resolved.Absolute) || Directory.Exists(resolved.Absolute);
        if (!exists)
        {
            sb.AppendLine("Type: not_found");
            return sb.ToString().TrimEnd();
        }

        var info = new FileInfo(resolved.Absolute);
        var attrs = info.Attributes;
        var isSymlink = (attrs & FileAttributes.ReparsePoint) != 0;
        var isDirectory = (attrs & FileAttributes.Directory) != 0;

        if (isDirectory)
        {
            AppendDirectoryReport(sb, new DirectoryInfo(resolved.Absolute), isSymlink, ct);
        }
        else
        {
            AppendFileReport(sb, info, isSymlink, previewLines, ct);
        }

        return sb.ToString().TrimEnd();
    }

    private void AppendFileReport(StringBuilder sb, FileInfo info, bool isSymlink, int previewLines, CancellationToken ct)
    {
        sb.Append("Type: ").AppendLine(isSymlink ? "symlink (file)" : "file");
        sb.Append("Size: ").Append(info.Length).Append(" bytes (").Append(FormatSize(info.Length)).AppendLine(")");
        sb.Append("Modified: ").AppendLine(new DateTimeOffset(info.LastWriteTimeUtc, TimeSpan.Zero).ToString("o"));
        sb.Append("Mime: ").AppendLine(GuessMime(info.Name));

        if (info.Length == 0)
        {
            sb.AppendLine("Encoding: empty");
            sb.AppendLine("Lines: 0");
            return;
        }

        if (info.Length > _options.MaxPreviewBytes)
        {
            sb.AppendLine("Encoding: (skipped - file exceeds MaxPreviewBytes)");
            sb.AppendLine("Lines: (skipped - file exceeds MaxPreviewBytes)");
            sb.AppendLine();
            sb.Append("Preview skipped - file is larger than ")
              .Append(FormatSize(_options.MaxPreviewBytes)).AppendLine(".");
            return;
        }

        // Read once, classify, count, preview. Single pass keeps it cheap.
        byte[] bytes;
        try
        {
            bytes = File.ReadAllBytes(info.FullName);
        }
        catch (Exception ex)
        {
            sb.AppendLine("Encoding: (read failed)");
            sb.Append("Error: ").AppendLine(ex.Message);
            return;
        }
        ct.ThrowIfCancellationRequested();

        var isBinary = LooksBinary(bytes);
        if (isBinary)
        {
            sb.AppendLine("Encoding: binary");
            sb.AppendLine("Lines: (not text)");
            sb.AppendLine();
            sb.AppendLine("Preview skipped - file contains non-text bytes.");
            return;
        }

        // Strip a UTF-8 BOM if present so it doesn't survive into the preview.
        var start = (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) ? 3 : 0;
        var text = Encoding.UTF8.GetString(bytes, start, bytes.Length - start);

        var lines = text.Split('\n');
        // Treat a trailing newline as not adding an empty line (common convention).
        var lineCount = lines.Length > 0 && lines[^1].Length == 0 ? lines.Length - 1 : lines.Length;
        sb.Append("Encoding: utf-8");
        if (start > 0) sb.Append(" (BOM)");
        sb.AppendLine();
        sb.Append("Lines: ").AppendLine(lineCount.ToString());

        if (previewLines <= 0 || lineCount == 0) return;

        var displayLines = lines.Take(lineCount).ToArray();
        var n = Math.Min(previewLines, displayLines.Length);

        sb.AppendLine();
        sb.Append("--- head ").Append(n).AppendLine(" ---");
        for (var i = 0; i < n; i++) sb.AppendLine(displayLines[i].TrimEnd('\r'));

        // Show the tail only when there's distinct content from the head - no
        // point repeating lines when the whole file is shorter than 2N.
        if (displayLines.Length > n * 2)
        {
            var tailStart = displayLines.Length - n;
            sb.Append("--- tail ").Append(n).Append(" (from line ").Append(tailStart + 1).AppendLine(") ---");
            for (var i = tailStart; i < displayLines.Length; i++) sb.AppendLine(displayLines[i].TrimEnd('\r'));
        }
    }

    private void AppendDirectoryReport(StringBuilder sb, DirectoryInfo dir, bool isSymlink, CancellationToken ct)
    {
        sb.Append("Type: ").AppendLine(isSymlink ? "symlink (directory)" : "directory");
        sb.Append("Modified: ").AppendLine(new DateTimeOffset(dir.LastWriteTimeUtc, TimeSpan.Zero).ToString("o"));

        FileSystemInfo[] entries;
        try
        {
            entries = dir.GetFileSystemInfos();
        }
        catch (Exception ex)
        {
            sb.Append("Error: could not enumerate - ").AppendLine(ex.Message);
            return;
        }
        ct.ThrowIfCancellationRequested();

        var fileCount = entries.Count(e => (e.Attributes & FileAttributes.Directory) == 0);
        var dirCount = entries.Length - fileCount;
        sb.Append("Entries: ").Append(entries.Length)
          .Append(" (").Append(fileCount).Append(" file(s), ")
          .Append(dirCount).AppendLine(" dir(s))");

        if (entries.Length == 0) return;

        // Sort: dirs first, then files; alphabetical within each. Show a small
        // preview here - full listing belongs in list_dir.
        var preview = entries
            .OrderBy(e => (e.Attributes & FileAttributes.Directory) != 0 ? 0 : 1)
            .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .Take(10)
            .ToArray();

        sb.AppendLine();
        sb.Append("--- first ").Append(preview.Length).AppendLine(" entries ---");
        foreach (var e in preview)
        {
            var marker = (e.Attributes & FileAttributes.Directory) != 0 ? "d" : "-";
            sb.Append(marker).Append(' ').AppendLine(e.Name);
        }

        if (entries.Length > preview.Length)
        {
            sb.Append("(... ").Append(entries.Length - preview.Length).AppendLine(" more - use list_dir for full listing)");
        }
    }

    private static bool LooksBinary(byte[] bytes)
    {
        var sniffLen = Math.Min(bytes.Length, BinarySniffBytes);
        for (var i = 0; i < sniffLen; i++)
        {
            if (bytes[i] == 0) return true;
        }
        return false;
    }

    private static string GuessMime(string filename) => Path.GetExtension(filename).ToLowerInvariant() switch
    {
        ".txt" or ".log" => "text/plain",
        ".md" => "text/markdown",
        ".json" => "application/json",
        ".csv" => "text/csv",
        ".tsv" => "text/tab-separated-values",
        ".yml" or ".yaml" => "application/yaml",
        ".xml" => "application/xml",
        ".html" or ".htm" => "text/html",
        ".css" => "text/css",
        ".js" or ".jsx" or ".mjs" or ".cjs" => "text/javascript",
        ".ts" or ".tsx" => "text/typescript",
        ".cs" => "text/x-csharp",
        ".py" => "text/x-python",
        ".java" => "text/x-java",
        ".go" => "text/x-go",
        ".rs" => "text/x-rust",
        ".rb" => "text/x-ruby",
        ".php" => "text/x-php",
        ".sql" => "application/sql",
        ".sh" or ".bash" or ".zsh" => "text/x-shellscript",
        ".ps1" => "text/x-powershell",
        ".bat" or ".cmd" => "text/x-batch",
        ".toml" => "application/toml",
        ".ini" or ".cfg" or ".conf" => "text/plain",
        ".pdf" => "application/pdf",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".svg" => "image/svg+xml",
        ".webp" => "image/webp",
        ".zip" => "application/zip",
        ".gz" => "application/gzip",
        _ => "application/octet-stream",
    };

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KiB",
        < 1024L * 1024 * 1024 => $"{bytes / (1024.0 * 1024.0):F1} MiB",
        _ => $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GiB",
    };
}
