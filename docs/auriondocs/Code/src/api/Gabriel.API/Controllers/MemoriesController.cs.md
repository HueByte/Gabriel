# MemoriesController

> **File:** `src/api/Gabriel.API/Controllers/MemoriesController.cs`  
> **Kind:** class

```csharp
[ApiController]
[Authorize]
[Route("memories")]
public class MemoriesController : ControllerBase
```


MemoriesController exposes a CRUD-style API surface over IMemoryService for managing memory entries used by the settings UI. It provides endpoints to list memories (with project-scoped or merged views), fetch a memory by id, upsert memories via a single POST path, and delete memories. The controller maps domain MemoryEntry objects to MemoryDto for transport and enforces validation on incoming SaveMemoryRequest (non-null body, a valid MemoryEntryType, and non-empty Name, Description, and Body).

## Remarks
This controller decouples the HTTP surface from the domain and persistence logic, centralizing request validation, routing, and DTO mapping while delegating storage and business rules to IMemoryService. The List endpoint supports a scope parameter that enables per-project views or a merged "all" view that mirrors what the agent sees, providing a consistent memory view for both UI and automation. The Save endpoint is designed to be idempotent: repeated upserts with the same identity update metadata rather than creating duplicates, aligning with the agent's memory_save workflow.

## Notes
- Type parsing is case-insensitive and restricted to the values: user, feedback, project, reference. If parsing fails, the endpoint returns 400 with a helpful error.
- Name, Description, and Body must be non-empty; otherwise the endpoint returns 400 Bad Request.
- Delete returns 204 No Content on success and 404 NotFound if the memory does not exist.