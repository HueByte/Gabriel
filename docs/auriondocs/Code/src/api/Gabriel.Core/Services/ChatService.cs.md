# ChatService

> **File:** `src/api/Gabriel.Core/Services/ChatService.cs`  
> **Kind:** class

Provides conversation management operations for the authenticated user: creating (with project resolution), listing, retrieving (with messages), renaming, re-rolling avatars, setting skins and modes, and other conversation lifecycle actions. Use this service from API controllers or application services when you need the business-level behavior (ownership checks, default-project resolution, domain validation and persistence) instead of calling repositories directly.

## Remarks
This class coordinates multiple collaborators: IConversationRepository for conversation queries/updates, IProjectRepository and IProjectService for project lookup and default-project resolution, IUnitOfWork for committing changes, and ICurrentUser to determine the acting user. It delegates state changes and validation to the Conversation domain object (for example Rename, RerollAvatar, SetSkin, SetMode) and ensures those changes are persisted via the unit-of-work.

## Example
```csharp
// Typical usage from an API controller
public async Task<IActionResult> Create([FromServices] ChatService chatService, Guid? projectId, string? title)
{
    var conversation = await chatService.CreateConversationAsync(projectId, title);
    return CreatedAtAction(nameof(Get), new { id = conversation.Id }, conversation);
}
```

## Notes
- Methods throw NotFoundException when a referenced project or conversation doesn't exist or isn't accessible to the current user.
- The service relies on a RequireUserId() call (via ICurrentUser) — callers must be authenticated; absence of a current user will result in an authorization failure.
- Conversation.Rename can throw ArgumentException for empty/whitespace titles; the surrounding infrastructure maps that to a 400 Bad Request.
- Changes are saved by calling IUnitOfWork.SaveChangesAsync; operations that update entities call Update on the repository before committing.
- SetSkin accepts nullable pattern and palette values (null can be used to clear values).
