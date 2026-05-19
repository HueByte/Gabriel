using Gabriel.Core.Exceptions;
using Gabriel.Core.Services;
using Microsoft.Extensions.Options;

namespace Gabriel.Engine.Tools.Files;

// Scoped - pulls IToolExecutionContext (per-turn project id) + IProjectFileService
// (project root + authz). Stateless aside from those.
public sealed class AgentPathResolver : IAgentPathResolver
{
    private readonly IToolExecutionContext _context;
    private readonly IProjectFileService _projectFiles;
    private readonly AgentToolsOptions _options;

    public AgentPathResolver(
        IToolExecutionContext context,
        IProjectFileService projectFiles,
        IOptions<AgentToolsOptions> options)
    {
        _context = context;
        _projectFiles = projectFiles;
        _options = options.Value;
    }

    public async Task<ResolvedPath> ResolveAsync(string relativeOrAbsolute, PathRootMode mode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(relativeOrAbsolute))
            throw new DomainException("Path cannot be empty.");

        var rootAbsolute = await ResolveRootAsync(mode, ct);

        // Path.GetFullPath joins relative paths against the root and canonicalizes
        // `..` segments. Absolute inputs are taken verbatim, then validated.
        var absolute = Path.IsPathRooted(relativeOrAbsolute)
            ? Path.GetFullPath(relativeOrAbsolute)
            : Path.GetFullPath(Path.Combine(rootAbsolute, relativeOrAbsolute));

        EnsureWithinRoot(absolute, rootAbsolute);

        var display = MakeRelativeDisplay(absolute, rootAbsolute);
        return new ResolvedPath(absolute, display, rootAbsolute, mode);
    }

    private async Task<string> ResolveRootAsync(PathRootMode mode, CancellationToken ct)
    {
        switch (mode)
        {
            case PathRootMode.Host:
                if (string.IsNullOrWhiteSpace(_options.HostRoot))
                    throw new DomainException(
                        "Host mode is disabled - operator hasn't set AgentTools:HostRoot. " +
                        "Use mode=\"project\" to access this conversation's project files instead.");
                return Path.GetFullPath(_options.HostRoot);

            case PathRootMode.Project:
                if (_context.ProjectId is not { } projectId)
                    throw new DomainException(
                        "This conversation isn't attached to a project - can't use project sandbox mode.");
                return await _projectFiles.GetProjectDirectoryAsync(projectId, ct);

            default:
                throw new DomainException($"Unknown path mode '{mode}'.");
        }
    }

    private static void EnsureWithinRoot(string absolute, string root)
    {
        // Append a separator so "C:\foo" doesn't accept "C:\foobar".
        var rootWithSep = root.EndsWith(Path.DirectorySeparatorChar) || root.EndsWith(Path.AltDirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;

        // Windows paths are case-insensitive; Linux they aren't. OrdinalIgnoreCase
        // is the safe-for-Windows choice and over-permissive on Linux only in the
        // case of mixed-case roots, which we don't generate.
        var cmp = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        if (!absolute.Equals(root, cmp) && !absolute.StartsWith(rootWithSep, cmp))
        {
            throw new DomainException($"Path '{absolute}' escapes the allowed root.");
        }
    }

    private static string MakeRelativeDisplay(string absolute, string root)
    {
        if (string.Equals(absolute, root, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            return ".";
        var rel = Path.GetRelativePath(root, absolute);
        // Path.GetRelativePath keeps native separators; normalize to forward
        // slashes so output is consistent across OSes (and copy-pasteable into
        // most tools).
        return rel.Replace('\\', '/');
    }
}
