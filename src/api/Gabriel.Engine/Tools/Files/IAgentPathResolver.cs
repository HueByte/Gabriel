namespace Gabriel.Engine.Tools.Files;

// Resolves a tool's `path` argument to an absolute path on disk, scoped to
// either the configured host root or the active conversation's project
// sandbox. All filesystem agent tools route through this so the same hardening
// (prefix check, mode gating, project authz) is enforced exactly once.
public interface IAgentPathResolver
{
    Task<ResolvedPath> ResolveAsync(string relativeOrAbsolute, PathRootMode mode, CancellationToken ct);
}

public enum PathRootMode
{
    Host,
    Project,
}

// `Display` is the user-friendly form to show in tool output — relative to
// the resolved root. `Absolute` is what we actually open. `RootAbsolute` is
// the directory the path is pinned under, surfaced for "Path: X (rooted at Y)"
// output and for callers that want to do their own walks.
public sealed record ResolvedPath(
    string Absolute,
    string Display,
    string RootAbsolute,
    PathRootMode Mode);
