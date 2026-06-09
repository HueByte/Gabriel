# ChatService

> **File:** `src/api/Gabriel.Core/Services/ChatService.cs`  
> **Kind:** class

A service that implements conversation-related use cases (creating, listing, fetching and updating conversations) and enforces that operations run in the context of the current user. Reach for ChatService from controllers or other application-layer components when you need to perform end-to-end conversation operations that involve authorization, project resolution and persistence rather than manipulating repositories or domain entities directly.

## Remarks
ChatService is an orchestration layer: it coordinates repository reads/writes, the project service, the current-user context and the unit-of-work to implement high-level conversation scenarios. Domain rules and validation (for example title validation or avatar/skin logic) are delegated to the Conversation entity; ChatService performs existence checks, resolves the target project (including ensuring a default project when no projectId is supplied), updates the repository and commits via IUnitOfWork.

## Example
```csharp
// Typical use from an API controller or application service
var created = await chatService.CreateConversationAsync(projectId: null, title: "Ideas for Q3", ct);
var all = await chatService.ListConversationsAsync(projectId: created.ProjectId, ct);
var renamed = await chatService.RenameConversationAsync(created.Id, "Updated title", ct);
```

## Notes
- Methods throw NotFoundException when the requested project or conversation does not exist for the current user.
- RenameConversationAsync delegates validation to Conversation.Rename, which throws ArgumentException on empty/whitespace titles (the comment in-source expects a global handler to map that to a 400 response).
- All operations call RequireUserId() to enforce an authenticated user context and use IUnitOfWork.SaveChangesAsync to persist updates; operations accept a CancellationToken and respect it.