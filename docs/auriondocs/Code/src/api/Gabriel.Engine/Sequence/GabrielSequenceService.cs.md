# GabrielSequenceService

> **File:** `src/api/Gabriel.Engine/Sequence/GabrielSequenceService.cs`  
> **Kind:** class

Produces a GabrielSequence for a conversation or a project by resolving the necessary domain state, enforcing that a user is authenticated, and delegating the final construction to an IGabrielSequenceGenerator. Reach for this service when callers need a ready-to-use sequence (with avatar seed, pattern/palette overrides and the most relevant live state) without pulling repositories and resolving fallback rules themselves.

## Remarks
This class centralizes the orchestration between conversation/project repositories, the current user context, and the sequence generator. For conversations it deliberately uses GetByIdWithMessagesAsync so the aggregate state and message history are available (the state is stored on the Conversation aggregate but message history can affect context). For projects it selects the latest conversation (repositories return conversations ordered by UpdatedAt DESC) and uses that conversation's state when present; otherwise the generator's neutral defaults are used. It also consistently enforces authentication and maps missing entities to NotFoundException.

## Example
```csharp
// Get a sequence for a single conversation
var sequence = await gabrielSequenceService.GetForConversationAsync(conversationId, cancellationToken);

// Get a sequence for a project (uses the latest conversation in the project if any)
var projectSequence = await gabrielSequenceService.GetForProjectAsync(projectId, cancellationToken);

// Handle common failures
try
{
    var seq = await gabrielSequenceService.GetForConversationAsync(conversationId);
}
catch (UnauthorizedAccessException)
{
    // user must be authenticated
}
catch (NotFoundException ex)
{
    // conversation or project not found
}
```

## Notes
- Both methods require an authenticated user; if CurrentUser.UserId is null an UnauthorizedAccessException is thrown.
- If the requested conversation or project can't be found the service throws NotFoundException.
- GetByIdWithMessagesAsync can be more expensive than a plain aggregate fetch because it includes message history; callers should be aware of potential performance/caching implications.
- GetForProjectAsync relies on the repository's ListAsync ordering contract (UpdatedAt DESC) to pick the "latest" conversation. If that contract changes, the selection logic will be affected.
- Both methods are asynchronous and accept a CancellationToken.