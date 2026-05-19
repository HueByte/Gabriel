using Gabriel.Core.Entities;

namespace Gabriel.Core.Services;

// File operations scoped to a single project. Path traversal is hardened
// (no `..`, no absolute paths, must resolve under the project's root dir).
// Read/list/delete are explicit; the upload path takes a stream + content
// type so the controller doesn't need to load the whole file into memory.
public interface IProjectFileService
{
    Task<IReadOnlyList<ProjectFile>> ListAsync(Guid projectId, CancellationToken ct = default);

    Task<ProjectFile> GetAsync(Guid projectId, Guid fileId, CancellationToken ct = default);

    // Returns the file metadata + an open Stream the caller MUST dispose.
    // Used by the download controller and the read_project_file tool.
    Task<(ProjectFile File, Stream Content)> OpenAsync(Guid projectId, Guid fileId, CancellationToken ct = default);

    // Reads the file as UTF-8 text up to `maxBytes`. Used by the read tool —
    // refuses non-text content types so a 200 MB ZIP doesn't blow the model's
    // context. Returns null if the file isn't text-like.
    Task<string?> ReadTextAsync(Guid projectId, Guid fileId, int maxBytes, CancellationToken ct = default);

    Task<ProjectFile> UploadAsync(
        Guid projectId,
        string filename,
        string? contentType,
        Stream content,
        CancellationToken ct = default);

    Task DeleteAsync(Guid projectId, Guid fileId, CancellationToken ct = default);

    // Authorizes the current user against the project, then returns the
    // absolute on-disk directory the project's uploaded files live in. Used
    // by agent filesystem tools to scope path-based reads to a project
    // sandbox (`{Root}/{projectId:N}`). Throws if the user can't access the
    // project — callers don't need to layer their own authz on top.
    Task<string> GetProjectDirectoryAsync(Guid projectId, CancellationToken ct = default);
}
