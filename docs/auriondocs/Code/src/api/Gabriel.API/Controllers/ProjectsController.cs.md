# ProjectsController

> **File:** `src/api/Gabriel.API/Controllers/ProjectsController.cs`  
> **Kind:** class

An API controller that exposes HTTP endpoints for managing projects and related project-level operations (list, get, create, update/patch, delete), plus project-scoped avatar and Gabriel sequence operations. It's the surface layer that authenticates requests, delegates business logic to IProjectService and IGabrielSequenceService, and maps domain models to ProjectResponse/GabrielSequenceResponse DTOs.

## Remarks
This controller is deliberately thin: it enforces routing and authorization, performs basic request-to-service translation, and returns standardized HTTP responses while leaving business rules to the injected services. Patch semantics are implemented at the controller level by checking which DTO properties are non-null and calling the appropriate service methods; see Notes for a caveat about JSON deserialization. The sequence endpoint aggregates project avatar seed and live conversation state via IGabrielSequenceService so callers can obtain the project-wide Gabriel sequence (clients should prefer this for non-default projects).

## Example
```csharp
// Create a new project
var client = new HttpClient { BaseAddress = new Uri("https://api.example.com/") };
var createReq = new
{
    Name = "My Project",
    Description = "Optional description",
    SystemPrompt = "You are a helpful assistant."
};
var createResp = await client.PostAsJsonAsync("projects", createReq);
createResp.EnsureSuccessStatusCode();
var project = await createResp.Content.ReadFromJsonAsync<ProjectResponse>();

// Get project sequence
var sequenceResp = await client.GetAsync($"projects/{project.Id}/sequence");
sequenceResp.EnsureSuccessStatusCode();
var sequence = await sequenceResp.Content.ReadFromJsonAsync<GabrielSequenceResponse>();
```

## Notes
- PATCH null ambiguity: the Update endpoint treats any null property on the request DTO as "do not change" or "clear" in a simplified way, but JSON deserialization cannot distinguish a missing key from an explicit null. The code currently treats both as null — review the PATCH design note if you need explicit-clear semantics.
- All endpoints require an authenticated user due to the [Authorize] attribute on the controller; unauthenticated requests will be rejected before reaching service methods.
- Rerolling the avatar only changes seed-derived dimensions; any pinned pattern/palette identifiers (if set) are preserved. The sequence endpoint is intended for non-default projects — default projects fall back to per-conversation sequences.