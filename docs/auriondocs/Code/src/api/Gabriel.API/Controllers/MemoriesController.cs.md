# MemoriesController

> **File:** `src/api/Gabriel.API/Controllers/MemoriesController.cs`  
> **Kind:** class

```csharp
[ApiController]
[Authorize]
[Route("memories")]
public class MemoriesController : ControllerBase
```


MemoriesController exposes a REST API for managing memory entries via an IMemoryService, providing listing, upserting (create/update), and deletion for the settings page. Clients can list memories with optional scope and project filters, retrieve a single memory by ID, upsert memory data with POST /memories, and delete memories by ID. The Save method validates the request (ensuring a valid MemoryEntryType, and non-empty Name, Description, and Body) and maps it into a MemoryEntrySpec before invoking the service. As noted in the code comments, upserts are keyed by (UserId, ProjectId, Name) and sending the same request twice simply updates the UpdatedAt timestamp, making the endpoint idempotent.

## Remarks
This controller acts as the HTTP boundary and DTO translator for memory management, keeping UI concerns separate from the domain service and providing a thin, idempotent surface over IMemoryService.