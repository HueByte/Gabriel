using Gabriel.Core.Entities;
using Gabriel.Core.Exceptions;
using Gabriel.Core.Identity;
using Gabriel.Core.Memory;
using Gabriel.Core.Repositories;

namespace Gabriel.Core.Services;

public class MemoryService : IMemoryService
{
    private readonly IMemoryRepository _memories;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly ISemanticMemoryIndex _index;

    public MemoryService(
        IMemoryRepository memories,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        ISemanticMemoryIndex index)
    {
        _memories = memories;
        _uow = uow;
        _currentUser = currentUser;
        _index = index;
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

        // Index after the relational save commits - SQLite is the source of
        // truth and the index contract is non-throwing, so a Qdrant outage
        // can't fail the save.
        await _index.UpsertAsync(entry, ct);
        return entry;
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await _memories.GetByIdAsync(id, RequireUserId(), ct);
        if (entry is null) return false;

        _memories.Remove(entry);
        await _uow.SaveChangesAsync(ct);
        await _index.DeleteAsync(id, ct);
        return true;
    }

    public async Task<bool> RemoveByNameAsync(Guid? projectId, string name, CancellationToken ct = default)
    {
        var entry = await _memories.FindByNameAsync(RequireUserId(), projectId, name, ct);
        if (entry is null) return false;

        var entryId = entry.Id;
        _memories.Remove(entry);
        await _uow.SaveChangesAsync(ct);
        await _index.DeleteAsync(entryId, ct);
        return true;
    }

    private Guid RequireUserId() =>
        _currentUser.UserId ?? throw new UnauthorizedAccessException("Authenticated user required.");
}
