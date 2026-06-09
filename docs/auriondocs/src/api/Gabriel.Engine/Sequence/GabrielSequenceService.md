# GabrielSequenceService

> **File:** `src/api/Gabriel.Engine/Sequence/GabrielSequenceService.cs`  
> **Kind:** class

Creates GabrielSequence instances for a conversation or a project by combining persistent seeds and overrides with the conversation's live state. Use this service when the UI or other parts of the system need the rendered/serializable sequence used to drive Gabriel avatars — it centralizes fetching the required aggregates, enforcing that a user is authenticated, and delegating sequence composition to an IGabrielSequenceGenerator.

## Remarks
This service sits between the domain repositories and the sequence generator. It enforces authentication (via ICurrentUser), resolves the correct aggregate state (conversation or the latest conversation for a project), and applies project-level overrides (pattern/palette) and avatar seeds before calling the generator. For conversations it intentionally requests message history (GetByIdWithMessagesAsync) so the resulting sequence can reflect live context; for projects it picks the most recently updated conversation to represent current live state. The generator is responsible for handling a null state or missing conversations by falling back to neutral defaults.

## Example
```csharp
// Resolve via DI, then use:
var sequence = await gabrielSequenceService.GetForConversationAsync(conversationId, cancellationToken);
// or for a project (will use the latest conversation in the project if present):
var projectSequence = await gabrielSequenceService.GetForProjectAsync(projectId, cancellationToken);
```

## Notes
- If no authenticated user is available an UnauthorizedAccessException is thrown.
- A NotFoundException is thrown when the requested Conversation or Project does not exist or is inaccessible to the current user.
- GetForProjectAsync selects the latest conversation returned by the conversation repository (the repository's ListAsync is expected to return conversations ordered by UpdatedAt DESC). If a project has no conversations the generator receives a null state and should apply neutral defaults.
- Fetching conversation messages on every call may be costly; callers or implementers may want to introduce caching or a cache-aware repository if this becomes a performance concern.
