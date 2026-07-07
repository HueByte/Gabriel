# IProjectRepository

> **File:** `src/api/Gabriel.Core/Repositories/IProjectRepository.cs`  
> **Kind:** interface

```csharp
public interface IProjectRepository
```


Represents a repository contract for Project entities, enabling asynchronous CRUD and query operations scoped to a particular owner. Use this instead of issuing direct data queries to encapsulate persistence details and to enable test doubles; for example, use GetByIdAsync / GetByIdWithFilesAsync to fetch a project, ListAsync to enumerate a user's projects, and AddAsync / Update / Remove to mutate state, while AssignOrphanConversationsAsync supports backfilling orphaned conversations to a given project.

## Remarks

This interface decouples the domain from the persistence mechanism, allowing the data store to change without impacting business logic. The GetByIdWithFilesAsync method indicates the repository can load related File entities alongside the project to satisfy scenarios where file associations are needed immediately. The special bulk operation AssignOrphanConversationsAsync addresses a lazy backfill pattern, enabling safety around interdependent data when a user first interacts with their Default project.

## Notes
- GetByIdWithFilesAsync loads related File entities; expect a heavier query and adjust projection accordingly.
- Update(Project) is synchronous and void; its persistence is typically coordinated via a unit of work or SaveChanges in the underlying data store.
- AssignOrphanConversationsAsync can touch a large portion of a user's conversations; consider batching or long-running task considerations in implementations.