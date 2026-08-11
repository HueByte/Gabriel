# Adding a new repository

> *Workflow template auto-derived from 4 existing exemplar(s).*

Adding a new repository in this codebase means adding a repository contract that expresses the data-access surface for a domain concept and then providing an implementation and registering it where implementations are composed. Reach for this pattern when you need a typed place to encapsulate CRUD and query operations for a domain entity so services can depend on an abstraction rather than concrete data-access code.

## Reference implementation

Real code from `src/api/Gabriel.Core/Repositories/IProjectRepository.cs` that a new instance can be modelled on:

```csharp
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
```

## Where it lives

Repository interfaces and their implementations are colocated under the repository area shown by the exemplars: the folder path is `src/api/Gabriel.Core/Repositories`. The existing interfaces use an `I{Name}Repository` naming style such as `IConversationRepository`, `IMemoryRepository`, `IMetricRepository`, and `IProjectRepository`, so add the new repository interface into that folder and follow the same interface name pattern.

## Wiring

Registration and composition points detected in this codebase where repository implementations are referenced or composed are:

- `src/api/Gabriel.Infrastructure/DependencyInjection.cs`
- `src/api/Gabriel.Core/Services/ChatService.cs`
- `src/api/Gabriel.Engine/Sequence/GabrielSequenceService.cs`

Use these wiring sites as the places to add or inspect registrations and where services consume repositories; the exemplars show how repository abstractions are referenced across the codebase.

## Existing examples

- [`IConversationRepository`](../../Code/src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md)
- [`IMemoryRepository`](../../Code/src/api/Gabriel.Core/Repositories/IMemoryRepository.cs.md)
- [`IMetricRepository`](../../Code/src/api/Gabriel.Core/Repositories/IMetricRepository.cs.md)
- [`IProjectRepository`](../../Code/src/api/Gabriel.Core/Repositories/IProjectRepository.cs.md)

---
*Synthesised by Aurion on 2026-07-08 05:47:38 UTC*
