using Gabriel.Core.Entities;
using Gabriel.Core.Exceptions;
using Gabriel.Core.Identity;
using Gabriel.Core.Repositories;

namespace Gabriel.Core.Services;

public class MemoryService : IMemoryService
{
    private readonly IMemoryRepository _memories;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public MemoryService(
        IMemoryRepository memories,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _memories = memories;
        _uow = uow;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<MemoryEntry>> ListAsync(Guid? projectId, CancellationToken ct = default)
        => _memories.ListAsync(RequireUserId(), projectId, ct);

    public Task<IReadOnlyList<MemoryEntry>> ListForConversationAsync(Guid? projectId, CancellationToken ct = default)
        => _memories.ListForAgentAsync(RequireUserId(), projectId, ct);

    public async Task<MemoryEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _memories.GetByIdAsync(id, RequireUserId(), ct);

    public async Task<MemoryEntry> SaveAsync(MemoryEntrySpec spec, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var existing = await _memories.FindByNameAsync(userId, spec.ProjectId, spec.Name, ct);

        MemoryEntry entry;
        if (existing is null)
        {
            entry = MemoryEntry.Create(
                userId: userId,
                projectId: spec.ProjectId,
                type: spec.Type,
                name: spec.Name,
                description: spec.Description,
                body: spec.Body);
            await _memories.AddAsync(entry, ct);
        }
        else
        {
            existing.Update(spec.Type, spec.Description, spec.Body);
            _memories.Update(existing);
            entry = existing;
        }

        await _uow.SaveChangesAsync(ct);
        return entry;
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await _memories.GetByIdAsync(id, RequireUserId(), ct);
        if (entry is null) return false;

        _memories.Remove(entry);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RemoveByNameAsync(Guid? projectId, string name, CancellationToken ct = default)
    {
        var entry = await _memories.FindByNameAsync(RequireUserId(), projectId, name, ct);
        if (entry is null) return false;

        _memories.Remove(entry);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    private Guid RequireUserId() =>
        _currentUser.UserId ?? throw new UnauthorizedAccessException("Authenticated user required.");
}
