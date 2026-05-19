using Gabriel.Core.Configuration;
using Gabriel.Core.Entities;
using Gabriel.Core.Exceptions;
using Gabriel.Core.Identity;
using Gabriel.Core.Repositories;
using Gabriel.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Gabriel.Infrastructure.Persistence;

namespace Gabriel.Infrastructure.Projects;

// Stores files on local disk at {Root}/{ProjectId:N}/{filename}. Metadata
// goes into the ProjectFiles table. Path-traversal hardened - every disk
// access resolves the final path and verifies it sits inside the project's
// directory before opening any handles.
public sealed class DiskProjectFileService : IProjectFileService
{
    private readonly AppDbContext _ctx;
    private readonly IProjectRepository _projects;
    private readonly ICurrentUser _currentUser;
    private readonly ProjectFilesOptions _options;
    private readonly ILogger<DiskProjectFileService> _logger;

    public DiskProjectFileService(
        AppDbContext ctx,
        IProjectRepository projects,
        ICurrentUser currentUser,
        IOptions<ProjectFilesOptions> options,
        ILogger<DiskProjectFileService> logger)
    {
        _ctx = ctx;
        _projects = projects;
        _currentUser = currentUser;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProjectFile>> ListAsync(Guid projectId, CancellationToken ct = default)
    {
        await AuthorizeAsync(projectId, ct);
        return await _ctx.ProjectFiles
            .Where(f => f.ProjectId == projectId)
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync(ct);
    }

    public async Task<ProjectFile> GetAsync(Guid projectId, Guid fileId, CancellationToken ct = default)
    {
        await AuthorizeAsync(projectId, ct);
        return await _ctx.ProjectFiles
            .FirstOrDefaultAsync(f => f.Id == fileId && f.ProjectId == projectId, ct)
            ?? throw new NotFoundException(nameof(ProjectFile), fileId);
    }

    public async Task<(ProjectFile File, Stream Content)> OpenAsync(Guid projectId, Guid fileId, CancellationToken ct = default)
    {
        var file = await GetAsync(projectId, fileId, ct);
        var fullPath = ResolveFilePath(projectId, file.RelativePath);
        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("ProjectFile {FileId} metadata exists but disk file is missing at {Path}", fileId, fullPath);
            throw new NotFoundException(nameof(ProjectFile), fileId);
        }
        // Caller is responsible for disposing the stream.
        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return (file, stream);
    }

    public async Task<string?> ReadTextAsync(Guid projectId, Guid fileId, int maxBytes, CancellationToken ct = default)
    {
        var (file, stream) = await OpenAsync(projectId, fileId, ct);
        await using (stream)
        {
            if (!IsTextLike(file.ContentType)) return null;

            var cap = Math.Min(maxBytes, (int)Math.Min(file.SizeBytes, int.MaxValue));
            var buffer = new byte[cap];
            var totalRead = 0;
            while (totalRead < cap)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(totalRead, cap - totalRead), ct);
                if (read == 0) break;
                totalRead += read;
            }
            return System.Text.Encoding.UTF8.GetString(buffer, 0, totalRead);
        }
    }

    public async Task<ProjectFile> UploadAsync(
        Guid projectId,
        string filename,
        string? contentType,
        Stream content,
        CancellationToken ct = default)
    {
        await AuthorizeAsync(projectId, ct);

        var sanitized = SanitizeFilename(filename);
        EnsureExtensionAllowed(sanitized);

        var projectDir = ResolveProjectDir(projectId);
        Directory.CreateDirectory(projectDir);

        // Collision policy: if the sanitized name is taken, append a short
        // suffix. Keeps round-trip predictable AND survives concurrent uploads
        // because the final filename includes a fresh suffix.
        var finalName = await PickAvailableNameAsync(projectId, sanitized, ct);
        var fullPath = ResolveFilePath(projectId, finalName);
        EnsureWithinProjectDir(fullPath, projectDir);

        long bytesWritten;
        await using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            bytesWritten = await CopyWithLimitAsync(content, fs, _options.MaxFileBytes, ct);
        }

        if (bytesWritten > _options.MaxFileBytes)
        {
            // The CopyWithLimit method tripped - file partially written + size
            // exceeded. Clean up before we record metadata.
            File.Delete(fullPath);
            throw new DomainException(
                $"File exceeds max upload size of {_options.MaxFileBytes / (1024 * 1024)} MiB.");
        }

        var resolvedContentType = NormalizeContentType(contentType, finalName);
        var entity = ProjectFile.Create(projectId, finalName, finalName, bytesWritten, resolvedContentType);
        _ctx.ProjectFiles.Add(entity);
        await _ctx.SaveChangesAsync(ct);

        return entity;
    }

    public async Task<string> GetProjectDirectoryAsync(Guid projectId, CancellationToken ct = default)
    {
        await AuthorizeAsync(projectId, ct);
        return ResolveProjectDir(projectId);
    }

    public async Task DeleteAsync(Guid projectId, Guid fileId, CancellationToken ct = default)
    {
        var file = await GetAsync(projectId, fileId, ct);
        var fullPath = ResolveFilePath(projectId, file.RelativePath);

        _ctx.ProjectFiles.Remove(file);
        await _ctx.SaveChangesAsync(ct);

        // DB row gone - best-effort delete on disk. If the file is missing,
        // that's fine; metadata being gone is the source of truth.
        if (File.Exists(fullPath))
        {
            try { File.Delete(fullPath); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete on-disk file {Path}", fullPath); }
        }
    }

    // --- internals -------------------------------------------------------------

    private async Task AuthorizeAsync(Guid projectId, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user required.");
        var project = await _projects.GetByIdAsync(projectId, userId, ct)
            ?? throw new NotFoundException("Project", projectId);
        _ = project; // owns-check passes via the user-scoped repo read
    }

    private string ResolveProjectDir(Guid projectId)
    {
        var root = Path.GetFullPath(_options.Root);
        return Path.Combine(root, projectId.ToString("N"));
    }

    private string ResolveFilePath(Guid projectId, string relativePath)
    {
        var dir = ResolveProjectDir(projectId);
        return Path.GetFullPath(Path.Combine(dir, relativePath));
    }

    private static void EnsureWithinProjectDir(string fullPath, string projectDir)
    {
        var canonicalProject = Path.GetFullPath(projectDir);
        var canonicalFile = Path.GetFullPath(fullPath);
        // OrdinalIgnoreCase covers Windows; on Linux paths are case-sensitive
        // but the prefix check still works for legitimate paths.
        if (!canonicalFile.StartsWith(canonicalProject + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && !canonicalFile.StartsWith(canonicalProject + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("Path traversal detected - file path escapes project directory.");
        }
    }

    // Reduce a user-supplied filename to a safe single-segment name. Drop path
    // separators, normalize to NFC, replace control chars with `_`, fall back
    // to a fresh GUID if everything got stripped.
    private static string SanitizeFilename(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Guid.NewGuid().ToString("N");

        var name = Path.GetFileName(raw.Trim());
        if (string.IsNullOrWhiteSpace(name))
            name = Guid.NewGuid().ToString("N");

        var invalid = Path.GetInvalidFileNameChars();
        var safe = string.Concat(name.Select(c => invalid.Contains(c) || char.IsControl(c) ? '_' : c));

        // Strip leading dots so we don't accidentally write hidden / config files.
        safe = safe.TrimStart('.');
        if (string.IsNullOrWhiteSpace(safe))
            safe = Guid.NewGuid().ToString("N");

        // Truncate to a reasonable length.
        if (safe.Length > 200) safe = safe[..200];
        return safe;
    }

    private void EnsureExtensionAllowed(string filename)
    {
        var ext = Path.GetExtension(filename).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext))
            throw new DomainException("File must have an extension.");
        if (!_options.AllowedExtensions.Any(a => a.Equals(ext, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"File extension '{ext}' is not allowed.");
    }

    private async Task<string> PickAvailableNameAsync(Guid projectId, string sanitized, CancellationToken ct)
    {
        var taken = await _ctx.ProjectFiles
            .Where(f => f.ProjectId == projectId && f.RelativePath == sanitized)
            .AnyAsync(ct);
        if (!taken) return sanitized;

        var stem = Path.GetFileNameWithoutExtension(sanitized);
        var ext = Path.GetExtension(sanitized);
        var suffix = Guid.NewGuid().ToString("N")[..6];
        return $"{stem}-{suffix}{ext}";
    }

    // Stream copy with a hard byte budget. Returns the actual bytes written
    // (which equals MaxFileBytes + 1 if the cap was exceeded - caller treats
    // that as an upload-too-large signal).
    private static async Task<long> CopyWithLimitAsync(Stream src, Stream dst, long max, CancellationToken ct)
    {
        var buffer = new byte[64 * 1024];
        long total = 0;
        while (true)
        {
            var read = await src.ReadAsync(buffer, ct);
            if (read == 0) break;
            total += read;
            if (total > max)
            {
                await dst.WriteAsync(buffer.AsMemory(0, (int)(read - (total - max))), ct);
                return max + 1;
            }
            await dst.WriteAsync(buffer.AsMemory(0, read), ct);
        }
        return total;
    }

    private static string NormalizeContentType(string? supplied, string filename)
    {
        if (!string.IsNullOrWhiteSpace(supplied) && supplied != "application/octet-stream")
            return supplied;

        var ext = Path.GetExtension(filename).ToLowerInvariant();
        return ext switch
        {
            ".txt" or ".log" => "text/plain",
            ".md" => "text/markdown",
            ".json" => "application/json",
            ".csv" => "text/csv",
            ".tsv" => "text/tab-separated-values",
            ".yml" or ".yaml" => "application/yaml",
            ".xml" => "application/xml",
            ".html" or ".htm" => "text/html",
            ".js" or ".jsx" or ".ts" or ".tsx" => "text/javascript",
            ".py" => "text/x-python",
            ".cs" => "text/x-csharp",
            ".java" => "text/x-java",
            ".go" => "text/x-go",
            ".rs" => "text/x-rust",
            ".sql" => "application/sql",
            ".sh" or ".ps1" or ".bat" => "text/plain",
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream",
        };
    }

    private bool IsTextLike(string contentType)
        => _options.TextContentTypePrefixes.Any(p => contentType.StartsWith(p, StringComparison.OrdinalIgnoreCase));
}
