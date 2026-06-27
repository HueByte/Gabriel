# MemoriesController

> **File:** `src/api/Gabriel.API/Controllers/MemoriesController.cs`  
> **Kind:** class

Provides an authenticated HTTP CRUD surface over IMemoryService for managing "memories" used by the settings UI and the agent. Use this controller when you need an HTTP endpoint for listing, creating/updating (upsert), retrieving or deleting memory entries; call IMemoryService directly when you are already inside server-side code and do not need the HTTP layer.

## Remarks
This controller centralizes the memory-related HTTP behavior and applies consistent validation and idempotency rules so both the UI and the agent use the same endpoint. The POST action is an upsert: the natural key is (UserId, ProjectId, Name) so repeating the same save request updates the existing entry's UpdatedAt rather than creating duplicates. The List endpoint supports a scope query parameter: omitting projectId returns user-scoped entries, providing a projectId returns that project's entries, and passing the literal string "all" for scope merges user-scope and project entries (the same view the agent sees for a conversation). All endpoints require an authenticated user ([Authorize]).

## Example
```csharp
// Common usage from a C# client using HttpClient
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("https://api.example.com/") };
client.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "<token>");

// List memories merged for a project
var projectId = Guid.Parse("11111111-2222-3333-4444-555555555555");
var list = await client.GetFromJsonAsync<List<MemoryDto>>($"memories?scope=all&projectId={projectId}");

// Upsert (save) a memory
var saveRequest = new {
    ProjectId = projectId,
    Type = "project",
    Name = "ImportantFact",
    Description = "Short description",
    Body = "Detailed memory body"
};
var saveResp = await client.PostAsJsonAsync("memories", saveRequest);
saveResp.EnsureSuccessStatusCode();
var saved = await saveResp.Content.ReadFromJsonAsync<MemoryDto>();

// Get by id
var single = await client.GetFromJsonAsync<MemoryDto>($"memories/{saved.Id}");

// Delete
var deleteResp = await client.DeleteAsync($"memories/{saved.Id}");
if (deleteResp.IsSuccessStatusCode) Console.WriteLine("Deleted");
```

## Notes
- The POST request validates Type against the MemoryEntryType enum (case-insensitive). Allowed values are: user, feedback, project, reference; invalid values produce a BadRequest with an explanatory error.
- Name, Description, and Body must be non-empty; otherwise the endpoint returns BadRequest.
- GET by id returns 404 when not found; DELETE returns 204 NoContent on success or 404 when the id does not exist. CancellationToken passed to each action is forwarded to the IMemoryService calls.