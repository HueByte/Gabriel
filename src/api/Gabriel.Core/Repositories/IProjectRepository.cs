using Gabriel.Core.Entities;

namespace Gabriel.Core.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, Guid ownerUserId, CancellationToken ct = default);
    Task<Project?> GetByIdWithFilesAsync(Guid id, Guid ownerUserId, CancellationToken ct = default);
    Task<IReadOnlyList<Project>> ListAsync(Guid ownerUserId, CancellationToken ct = default);
    Task<Project?> GetFirstByNameAsync(Guid ownerUserId, string name, CancellationToken ct = default);

    Task AddAsync(Project project, CancellationToken ct = default);
    void Update(Project project);
    void Remove(Project project);

    // Bulk-assign every project-less conversation of a user to the given project.
    // Used by the Default-project lazy backfill on first project interaction.
    Task<int> AssignOrphanConversationsAsync(Guid ownerUserId, Guid projectId, CancellationToken ct = default);
}
