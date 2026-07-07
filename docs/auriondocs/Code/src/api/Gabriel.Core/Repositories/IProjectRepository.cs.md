# IProjectRepository

> **File:** `src/api/Gabriel.Core/Repositories/IProjectRepository.cs`  
> **Kind:** interface

```csharp
public interface IProjectRepository
```


IProjectRepository defines the persistence contract for Project entities. It exposes asynchronous retrieval methods to fetch a project by Id (optionally loading its files), to list projects for a specific owner, and to find a project by name, all scoped by ownerUserId to enforce per-owner data boundaries. It also provides mutating operations to add, update, or remove a project, and a bulk operation to assign orphan conversations to a project. Implementations may rely on a variety of storage technologies behind this interface, allowing domain logic to stay ignorant of persistence details.

## Remarks
The repository pattern here decouples domain logic from data access, enabling swapping the underlying storage without impacting business rules. By requiring ownerUserId on read paths, the API enforces per-tenant isolation and makes multi-tenant concerns explicit in the contract. The AssignOrphanConversationsAsync method supports backfill scenarios where conversations need to be associated with a project, returning a count of how many conversations were reassigned to help callers detect whether work occurred.

## Notes
- Update(Project project) is synchronous and does not accept a CancellationToken; long-running or I/O-bound updates may block callers unless the implementation performs asynchronous I/O internally.
- AddAsync accepts a CancellationToken while the other mutators do not; cancellation semantics depend on the concrete implementation and how it handles I/O.
- AssignOrphanConversationsAsync returns the number of conversations reassigned; a result of 0 indicates no changes were necessary.
