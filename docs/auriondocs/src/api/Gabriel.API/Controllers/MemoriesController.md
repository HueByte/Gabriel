# MemoriesController

> **File:** `src/api/Gabriel.API/Controllers/MemoriesController.cs`  
> **Kind:** class

Exposes a small CRUD HTTP surface for storing and retrieving user- and project-scoped memories. Use this controller when you need a simple, authenticated API for listing, creating/updating (upsert), fetching by id, and deleting memory entries; business logic and persistence are delegated to IMemoryService.

## Remarks
The controller centralizes memory-related endpoints and normalizes responses to MemoryDto. A single POST endpoint is used for both create and update (upsert) operations so clients and automated agents can call one idempotent entry point. The List endpoint accepts an optional scope and projectId — passing the literal string "all" for scope merges user-scope and project-specific entries via IMemoryService.ListForConversationAsync.

## Example
```csharp
// List all memories visible to the current user and a specific project (scope=all)
using var client = new HttpClient { BaseAddress = new Uri("https://api.example.com/") };
var listResponse = await client.GetAsync("memories?scope=all&projectId=00000000-0000-0000-0000-000000000000");
listResponse.EnsureSuccessStatusCode();
var listJson = await listResponse.Content.ReadAsStringAsync();

// Save (upsert) a memory
var saveBody = new
{
    projectId = (Guid?)null,
    type = "user", // must be one of: user, feedback, project, reference
    name = "Important Preference",
    description = "Stores the user's preferred language",
    body = "English"
};
var saveResponse = await client.PostAsJsonAsync("memories", saveBody);
saveResponse.EnsureSuccessStatusCode();
var savedJson = await saveResponse.Content.ReadAsStringAsync();

// Delete by id
var deleteResponse = await client.DeleteAsync($"memories/{Guid.Parse("11111111-1111-1111-1111-111111111111")}\");
if (deleteResponse.StatusCode == System.Net.HttpStatusCode.NoContent) {
    // removed
}
```

## Notes
- The controller requires authorization; requests must be authenticated.
- POST performs validation: Type must parse to MemoryEntryType (user, feedback, project, reference) and Name/Description/Body must be non-empty; BadRequest is returned on validation failure.
- The POST is an upsert that uses (UserId, ProjectId, Name) as the natural key — sending the same payload twice updates UpdatedAt rather than creating duplicates.
