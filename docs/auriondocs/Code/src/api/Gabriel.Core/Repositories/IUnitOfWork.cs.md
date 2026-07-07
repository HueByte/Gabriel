# IUnitOfWork

> **File:** `src/api/Gabriel.Core/Repositories/IUnitOfWork.cs`  
> **Kind:** interface

```csharp
public interface IUnitOfWork
```


IUnitOfWork coordinates persistence across repositories, allowing multiple in-memory changes to be committed as a single transaction. Call SaveChangesAsync after performing a set of repository operations to persist all changes atomically within one unit of work. The method is asynchronous and accepts a CancellationToken (defaulted), enabling callers to cancel the operation if it is taking too long or the request is aborted. The return value represents the number of entries persisted to the data store.

## Remarks
By encapsulating the commit step, this abstraction decouples business logic from the specifics of the data access technology. It centralizes transactional boundaries, making cross-repository consistency easier to reason about and testable via mocks or fakes. In typical implementations, the unit of work is registered with a per-operation lifetime so that all repository changes share the same transactional context.

## Example
```csharp
// After performing operations across repositories
int affected = await unitOfWork.SaveChangesAsync();
```

## Notes
- The return value indicates how many entries were persisted to the data store; a value of 0 may mean no tracked changes were applied.
- SaveChangesAsync may throw if the persistence layer reports an error; callers should handle or propagate exceptions to maintain transactional integrity.