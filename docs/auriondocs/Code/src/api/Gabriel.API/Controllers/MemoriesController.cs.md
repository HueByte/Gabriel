# MemoriesController

> **File:** `src/api/Gabriel.API/Controllers/MemoriesController.cs`  
> **Kind:** class

```csharp
[ApiController]
[Authorize]
[Route("memories")]
public class MemoriesController : ControllerBase
```


MemoriesController exposes a RESTful API surface for managing memory entries through IMemoryService. It supports listing memories (with an optional scope filter), retrieving a single memory by ID, upserting memories via a single idempotent POST, and deleting memories by ID. The controller validates input, converts domain entities to MemoryDto objects, and returns conventional HTTP responses.

## Remarks
As the HTTP façade over the memory domain, this controller centralizes memory-management concerns for both the settings UI and agent integrations. All write-paths funnel through the same Save endpoint, enabling consistent idempotent upserts keyed by UserId, ProjectId, and Name, while reads are shaped by the scope parameter to produce user-only, project-scoped, or merged views.

## Example
```csharp
// Common usage: upsert a memory (idempotent)
var req = new SaveMemoryRequest
{
    ProjectId = projectId,
    Type = "user",
    Name = "Onboarding notes",
    Description = "Guidance for onboarding users",
    Body = "Remember to collect feedback from new users and update the process."
};

// POST to /memories
var response = await httpClient.PostAsJsonAsync("memories", req);
```

## Notes
- Upsert validation: Name, Description, and Body must be non-empty; Type must be one of: user, feedback, project, reference; otherwise returns 400 with a specific error.
- Delete endpoint returns NoContent on success, NotFound if the ID does not exist; List supports scope filtering with an 'all' option to merge user-scope and project entries.
