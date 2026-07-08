# IMemoryRepository

> **File:** `src/api/Gabriel.Core/Repositories/IMemoryRepository.cs`  
> **Kind:** interface

```csharp
public interface IMemoryRepository
```


An abstraction for storing and querying MemoryEntry objects with user- and project-scoped visibility. Use this interface when code needs to list the memories visible to a user or agent, retrieve a specific memory, or create/update/remove memories without tying callers to a particular persistence technology.

## Remarks
This repository surface is scope-aware: methods accept a userId plus an optional projectId to target either user-scope (projectId == null) or a project-specific scope. ListForAgentAsync is a convenience that returns the set of entries an agent should "see" for a conversation — the user's global memories plus, when a projectId is supplied, that project's memories. FindByNameAsync looks up a memory by (userId, projectId, name); the implementation and callers treat the name/slug as unique within the chosen scope, which is why callers use it to decide between creating a new entry or updating an existing one.

## Notes
- Pass projectId = null to target the user scope; ListForAgentAsync merges user-scope and project-scope entries when a projectId is provided.
- FindByNameAsync relies on the name being unique within (userId, projectId) so callers (for example, an agent's memory-save logic) can choose create vs. update based on its result.
- AddAsync is asynchronous while Update and Remove are synchronous and the interface does not expose an explicit Save/Commit method; implementations will commonly rely on an ambient unit-of-work or change-tracking context to persist Update/Remove changes — make sure your implementation’s lifecycle persists those modifications as expected.