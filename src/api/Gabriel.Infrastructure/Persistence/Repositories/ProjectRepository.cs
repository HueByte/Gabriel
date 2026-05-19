using Gabriel.Core.Entities;
using Gabriel.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gabriel.Infrastructure.Persistence.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _ctx;

    public ProjectRepository(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public Task<Project?> GetByIdAsync(Guid id, Guid ownerUserId, CancellationToken ct = default)
        => _ctx.Projects.FirstOrDefaultAsync(p => p.Id == id && p.OwnerUserId == ownerUserId, ct);

    public Task<Project?> GetByIdWithFilesAsync(Guid id, Guid ownerUserId, CancellationToken ct = default)
        => _ctx.Projects
            .Include(p => p.Files.OrderByDescending(f => f.UploadedAt))
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerUserId == ownerUserId, ct);

    public async Task<IReadOnlyList<Project>> ListAsync(Guid ownerUserId, CancellationToken ct = default)
        => await _ctx.Projects
            .Where(p => p.OwnerUserId == ownerUserId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(ct);

    public Task<Project?> GetFirstByNameAsync(Guid ownerUserId, string name, CancellationToken ct = default)
        => _ctx.Projects
            .Where(p => p.OwnerUserId == ownerUserId && p.Name == name)
            .OrderBy(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(Project project, CancellationToken ct = default)
        => await _ctx.Projects.AddAsync(project, ct);

    public void Update(Project project) => _ctx.Projects.Update(project);

    public void Remove(Project project) => _ctx.Projects.Remove(project);

    public Task<int> AssignOrphanConversationsAsync(Guid ownerUserId, Guid projectId, CancellationToken ct = default)
    {
        // ExecuteUpdate — bulk reassign without loading rows into the change tracker.
        // Conversations whose ProjectId is null AND owned by this user → set to projectId.
        return _ctx.Conversations
            .Where(c => c.UserId == ownerUserId && c.ProjectId == null)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.ProjectId, projectId), ct);
    }
}
