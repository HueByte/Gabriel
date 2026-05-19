using Gabriel.Core.Entities;
using Gabriel.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gabriel.Infrastructure.Persistence.Repositories;

public class MemoryRepository : IMemoryRepository
{
    private readonly AppDbContext _ctx;

    public MemoryRepository(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IReadOnlyList<MemoryEntry>> ListAsync(Guid userId, Guid? projectId, CancellationToken ct = default)
    {
        return await _ctx.MemoryEntries
            .Where(m => m.UserId == userId && m.ProjectId == projectId)
            .OrderBy(m => m.Type)
            .ThenBy(m => m.Name)
            .ToListAsync(ct);
    }

    // The agent's view: user-scope + (optionally) one project's scope. Done as
    // a single query with an OR so the round-trip cost stays at one regardless
    // of which conversation is open.
    public async Task<IReadOnlyList<MemoryEntry>> ListForAgentAsync(Guid userId, Guid? projectId, CancellationToken ct = default)
    {
        IQueryable<MemoryEntry> q = _ctx.MemoryEntries.Where(m => m.UserId == userId);

        q = projectId is { } pid
            ? q.Where(m => m.ProjectId == null || m.ProjectId == pid)
            : q.Where(m => m.ProjectId == null);

        return await q
            .OrderBy(m => m.ProjectId == null ? 0 : 1)  // user-scope first, then project-scope
            .ThenBy(m => m.Type)
            .ThenBy(m => m.Name)
            .ToListAsync(ct);
    }

    public Task<MemoryEntry?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
        => _ctx.MemoryEntries.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId, ct);

    public Task<MemoryEntry?> FindByNameAsync(Guid userId, Guid? projectId, string name, CancellationToken ct = default)
        => _ctx.MemoryEntries.FirstOrDefaultAsync(
            m => m.UserId == userId && m.ProjectId == projectId && m.Name == name,
            ct);

    public async Task AddAsync(MemoryEntry entry, CancellationToken ct = default)
        => await _ctx.MemoryEntries.AddAsync(entry, ct);

    public void Update(MemoryEntry entry) => _ctx.MemoryEntries.Update(entry);

    public void Remove(MemoryEntry entry) => _ctx.MemoryEntries.Remove(entry);
}
