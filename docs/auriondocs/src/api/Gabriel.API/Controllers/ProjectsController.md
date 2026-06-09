# ProjectsController

> **File:** `src/api/Gabriel.API/Controllers/ProjectsController.cs`  
> **Kind:** class

Exposes CRUD and project-specific endpoints for managing Project entities and related per-project Gabriel sequences. Use this controller when building API handlers that create, read, update, delete or otherwise manipulate projects (including avatar operations and obtaining the project-level Gabriel sequence) — it delegates domain work to IProjectService and IGabrielSequenceService and returns ProjectResponse/GabrielSequenceResponse DTOs.

## Remarks
This controller is an API surface that maps HTTP routes under the "projects" base path to operations on the domain services. It centralizes HTTP concerns (routing, authorization, status codes, DTO projection) while delegating persistence and business logic to IProjectService and sequence aggregation to IGabrielSequenceService. Endpoints consistently use cancellation tokens and return HTTP-appropriate responses (Ok, CreatedAtAction, NoContent) and project responses are produced by calling the domain-to-DTO conversion (ToResponse).

## Notes
- All endpoints require authentication (the controller is decorated with [Authorize]).
- PATCH semantics: the Update action treats the incoming DTO as all-nullable. A non-null field will be applied; null is treated as an explicit clear for Description/SystemPrompt, while the implementation currently simplifies missing keys and explicit nulls similarly due to JSON deserialization. Consult the PATCH design note if you need explicit "clear vs missing" behavior.
- Create returns 201 Created using CreatedAtAction pointing to Get (Location header contains the new project's id).
- GetSequence returns the project-shared Gabriel sequence (aggregated from Project.AvatarSeed and the project's most-recent-live conversation); default-project behavior is different (client should call per-conversation sequences for default projects).
- RerollAvatar changes only the seed-derived avatar dimensions; pinned pattern/palette identifiers (if set) survive a reroll.
- All actions accept a CancellationToken and forward it to service calls; callers should propagate cancellation where appropriate.