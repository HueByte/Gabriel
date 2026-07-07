# Adding a new repository

> *Workflow template auto-derived from 4 existing exemplar(s).*

When you need to add a new repository abstraction to the codebase — for example to surface storage operations for a new aggregate or entity — follow the repository-pattern instances already present under Gabriel.Core. The examples below show the minimal surface area the rest of the code expects (async getters, list operations, Add/Update/Remove, and any bulk helpers). Model your new interface on the reference implementation and then wire it into the places where repositories are referenced.

## Reference implementation

Real code from src/api/Gabriel.Core/Repositories/IProjectRepository.cs that a new instance can be modeled on:

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

Repository interfaces live in the src/api/Gabriel.Core/Repositories folder. Existing repository interface names shown in the exemplars are IConversationRepository, IMemoryRepository, IMetricRepository, and IProjectRepository; use a similar name pattern (the interfaces start with `I` and end with `Repository`) so callers can find and consume the abstraction the same way the exemplars do.

## Wiring

The following files were detected as wiring sites or places that reference multiple repository exemplars; inspect them to see how repositories are referenced and to add your new repository where appropriate:

- src/api/Gabriel.Infrastructure/DependencyInjection.cs
- src/api/Gabriel.Core/Services/ChatService.cs
- src/api/Gabriel.Engine/Sequence/GabrielSequenceService.cs

Open those files and follow how the existing repository types are used and composed. The exemplars listed below show the interface shape to match; the wiring sites above are the places you will need to update or consult when connecting your new repository to the rest of the system.

## Existing examples

- [`IConversationRepository`](../../Code/src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md)
- [`IMemoryRepository`](../../Code/src/api/Gabriel.Core/Repositories/IMemoryRepository.cs.md)
- [`IMetricRepository`](../../Code/src/api/Gabriel.Core/Repositories/IMetricRepository.cs.md)
- [`IProjectRepository`](../../Code/src/api/Gabriel.Core/Repositories/IProjectRepository.cs.md)

---
*Synthesised by Aurion on 2026-07-07 21:09:18 UTC*
