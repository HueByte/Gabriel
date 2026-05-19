using Gabriel.Core.Entities;

namespace Gabriel.Core.Services;

public interface IProjectService
{
    // Returns the user's projects (only their own - multi-tenant scoped at the
    // repo level). Always lazily ensures a "Default" project exists; absorbs
    // pre-Phase-8 conversations into it on first call.
    Task<IReadOnlyList<Project>> ListAsync(CancellationToken ct = default);

    Task<Project> GetAsync(Guid id, CancellationToken ct = default);

    // Returns the project with its files loaded, sorted newest-first.
    Task<Project> GetWithFilesAsync(Guid id, CancellationToken ct = default);

    Task<Project> CreateAsync(string name, string? description, string? systemPrompt, CancellationToken ct = default);

    Task<Project> RenameAsync(Guid id, string name, CancellationToken ct = default);
    Task<Project> UpdateDescriptionAsync(Guid id, string? description, CancellationToken ct = default);
    Task<Project> UpdateSystemPromptAsync(Guid id, string? systemPrompt, CancellationToken ct = default);

    // Re-rolls the project's AvatarSeed so the seed-derived dimensions of the
    // sequence (pattern + palette, when not pinned via SetSkinAsync) change.
    Task<Project> RerollAvatarAsync(Guid id, CancellationToken ct = default);

    // Pin the project's avatar pattern + palette. Pass null or empty string
    // to clear an individual override and fall back to seed-derived behavior
    // for that dimension. Catalog identifiers are validated at the API layer
    // before reaching here.
    Task<Project> SetSkinAsync(Guid id, string? pattern, string? palette, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    // Get or lazily create the "Default" project for the current user. Returns
    // the project id. Also assigns any of the user's project-less conversations
    // (legacy data) to it.
    Task<Guid> EnsureDefaultProjectIdAsync(CancellationToken ct = default);
}
