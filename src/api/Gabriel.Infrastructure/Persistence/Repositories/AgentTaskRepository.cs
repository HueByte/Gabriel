using Gabriel.Core.Entities;
using Gabriel.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gabriel.Infrastructure.Persistence.Repositories;

public class AgentTaskRepository : IAgentTaskRepository
{
    private static readonly AgentTaskStatus[] DrivingStatuses = [AgentTaskStatus.Running];

    private readonly AppDbContext _ctx;

    public AgentTaskRepository(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IReadOnlyList<AgentTask>> ListAsync(Guid userId, Guid? projectId, CancellationToken ct = default)
    {
        var q = _ctx.AgentTasks.Where(t => t.UserId == userId);
        if (projectId is { } pid) q = q.Where(t => t.ProjectId == pid);
        return await q.OrderByDescending(t => t.UpdatedAt).ToListAsync(ct);
    }

    public Task<AgentTask?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
        => _ctx.AgentTasks
            .Include(t => t.Log.OrderBy(e => e.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);

    public Task<AgentTask?> FindByConversationAsync(Guid conversationId, CancellationToken ct = default)
        => _ctx.AgentTasks.FirstOrDefaultAsync(t => t.ConversationId == conversationId, ct);

    public Task<bool> IsConversationDrivenAsync(Guid conversationId, CancellationToken ct = default)
        => _ctx.AgentTasks.AnyAsync(t => t.ConversationId == conversationId && DrivingStatuses.Contains(t.Status), ct);

    public async Task<IReadOnlyList<(Guid TaskId, Guid UserId)>> ListRunnableAsync(CancellationToken ct = default)
    {
        var rows = await _ctx.AgentTasks
            .Where(t => t.Status == AgentTaskStatus.Running)
            .OrderBy(t => t.UpdatedAt)
            .Select(t => new { t.Id, t.UserId })
            .ToListAsync(ct);
        return rows.Select(r => (r.Id, r.UserId)).ToList();
    }

    public Task<AgentTask?> GetForWorkerAsync(Guid id, CancellationToken ct = default)
        => _ctx.AgentTasks
            .Include(t => t.Log.OrderBy(e => e.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task ReloadAsync(AgentTask task, CancellationToken ct = default)
        => _ctx.Entry(task).ReloadAsync(ct);

    public async Task AddAsync(AgentTask task, CancellationToken ct = default)
        => await _ctx.AgentTasks.AddAsync(task, ct);
}
