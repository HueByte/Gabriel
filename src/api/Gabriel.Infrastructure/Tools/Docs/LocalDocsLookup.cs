using Gabriel.Core.Configuration;
using Gabriel.Engine.Tools.Docs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gabriel.Infrastructure.Tools.Docs;

// IDocsLookup over the on-disk LLM-native self-docs folder. This is the
// PRIMARY docs source - pages here are written specifically for the model to
// consume and take precedence over the GitHub-backed fallback.
//
// Root resolution runs once on first use (lazy). When LocalDocsOptions.Path
// is absolute and exists, it is used as-is. Otherwise the resolver probes
// Environment.CurrentDirectory and AppContext.BaseDirectory, walking up a few
// parents looking for the relative path. First match wins. If nothing is
// found the source behaves as empty (zero entries, all reads return null) and
// the composite lookup will transparently fall back to the GitHub source.
public sealed class LocalDocsLookup : IDocsLookup
{
    // Cap on how far up we walk from each probe origin. The repo root is
    // typically 2-4 levels above any binary output dir, so 8 is generous.
    private const int MaxParentLevels = 8;

    // Pull the first H1 (`# ...`) line we encounter to populate DocsEntry.Title.
    // Tolerates a leading BOM and blank lines before the heading.
    private const int TitleScanByteLimit = 4096;

    private readonly LocalDocsOptions _options;
    private readonly ILogger<LocalDocsLookup> _logger;

    private readonly object _resolveLock = new();
    private bool _resolved;
    private string? _rootPath;

    public LocalDocsLookup(IOptions<LocalDocsOptions> options, ILogger<LocalDocsLookup> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<IReadOnlyList<DocsEntry>> ListAsync(CancellationToken ct)
    {
        if (!_options.Enabled) return Task.FromResult<IReadOnlyList<DocsEntry>>(Array.Empty<DocsEntry>());

        var root = ResolveRoot();
        if (root is null) return Task.FromResult<IReadOnlyList<DocsEntry>>(Array.Empty<DocsEntry>());

        var entries = new List<DocsEntry>();
        foreach (var fullPath in Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();

            // Path RELATIVE to the local root, using forward slashes so it
            // round-trips cleanly between docs_list output and docs_read input
            // regardless of platform.
            var rel = Path.GetRelativePath(root, fullPath).Replace('\\', '/');
            var title = TryReadFirstH1(fullPath);
            entries.Add(new DocsEntry(rel, title, DocsSources.LocalLlmNative));
        }

        return Task.FromResult<IReadOnlyList<DocsEntry>>(entries);
    }

    public async Task<DocsContent?> ReadAsync(string path, CancellationToken ct)
    {
        if (!_options.Enabled) return null;

        ValidatePath(path);

        var root = ResolveRoot();
        if (root is null) return null;

        // Normalize separators, then re-anchor at the root. Path.Combine + a
        // GetFullPath/StartsWith check defends against any cute traversal that
        // slipped past ValidatePath (e.g. symlink loops that resolve outward).
        var normalized = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var combined = Path.GetFullPath(Path.Combine(root, normalized));
        var rootFull = Path.GetFullPath(root);
        var rootWithSep = rootFull.EndsWith(Path.DirectorySeparatorChar)
            ? rootFull
            : rootFull + Path.DirectorySeparatorChar;
        if (!combined.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase))
            return null;

        if (!File.Exists(combined)) return null;

        var content = await File.ReadAllTextAsync(combined, ct);
        var canonicalUrl = new Uri(combined).AbsoluteUri;
        return new DocsContent(path, content, canonicalUrl, DocsSources.LocalLlmNative);
    }

    private string? ResolveRoot()
    {
        if (_resolved) return _rootPath;

        lock (_resolveLock)
        {
            if (_resolved) return _rootPath;

            var configured = _options.Path;
            if (string.IsNullOrWhiteSpace(configured))
            {
                _logger.LogWarning("Tools:Docs:Local:Path is empty; local self-docs source disabled.");
                _resolved = true;
                return null;
            }

            // Absolute path: take it at face value if it exists.
            if (Path.IsPathRooted(configured))
            {
                if (Directory.Exists(configured))
                {
                    _rootPath = Path.GetFullPath(configured);
                    _logger.LogInformation("Local self-docs root resolved (absolute): {Path}", _rootPath);
                }
                else
                {
                    _logger.LogWarning(
                        "Tools:Docs:Local:Path '{Path}' is absolute but does not exist; falling back to remote docs source.",
                        configured);
                }
                _resolved = true;
                return _rootPath;
            }

            // Relative: probe two origins and walk up. Process working directory
            // wins when the host is launched from the repo (the dev case);
            // BaseDirectory wins for published/single-file deployments where
            // the binary lives next to the docs folder.
            var probes = new[] { Environment.CurrentDirectory, AppContext.BaseDirectory };
            foreach (var origin in probes)
            {
                var found = WalkUpFor(origin, configured);
                if (found is not null)
                {
                    _rootPath = found;
                    _logger.LogInformation(
                        "Local self-docs root resolved by walking from {Origin}: {Path}",
                        origin, _rootPath);
                    _resolved = true;
                    return _rootPath;
                }
            }

            _logger.LogWarning(
                "Local self-docs path '{Path}' not found from CWD '{Cwd}' or BaseDir '{Base}'; falling back to remote docs source.",
                configured, Environment.CurrentDirectory, AppContext.BaseDirectory);
            _resolved = true;
            return null;
        }
    }

    private static string? WalkUpFor(string origin, string relative)
    {
        var current = new DirectoryInfo(origin);
        for (var i = 0; i < MaxParentLevels && current is not null; i++)
        {
            var candidate = Path.Combine(current.FullName, relative);
            if (Directory.Exists(candidate))
                return Path.GetFullPath(candidate);
            current = current.Parent;
        }
        return null;
    }

    private static string? TryReadFirstH1(string fullPath)
    {
        try
        {
            using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);
            var read = 0;
            string? line;
            while ((line = reader.ReadLine()) is not null && read < TitleScanByteLimit)
            {
                read += line.Length + 1;
                var trimmed = line.TrimStart('﻿', ' ', '\t');
                if (trimmed.StartsWith("# ", StringComparison.Ordinal))
                    return trimmed[2..].Trim();
            }
        }
        catch
        {
            // Title parsing is best-effort - falling back to null is fine.
        }
        return null;
    }

    private static void ValidatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is required.", nameof(path));

        if (path.StartsWith('/') || path.StartsWith('\\'))
            throw new ArgumentException("Path must be relative.", nameof(path));
        if (Path.IsPathRooted(path))
            throw new ArgumentException("Path must be relative.", nameof(path));
        if (path.Split('/', '\\').Any(seg => seg == ".." || seg == "."))
            throw new ArgumentException("Path may not contain '.' or '..' segments.", nameof(path));
    }
}
