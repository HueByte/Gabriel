# IGabrielSequenceService

> **File:** `src/api/Gabriel.Engine/Sequence/IGabrielSequenceService.cs`  
> **Kind:** interface

Provides a single, async integration point that produces a GabrielSequence for rendering an avatar-driven visual sequence. Use this service when a controller or other upstream component needs the fully-composed sequence for a specific conversation (user-scoped) or for an entire project (project-scoped) without needing to know about repositories, avatar seeds, or how conversation state is aggregated.

## Remarks
This interface centralizes the logic that loads the relevant AvatarSeed and ConversationState, then hands them to the generator to produce a GabrielSequence. The conversation variant returns a sequence scoped to a single conversation's state, while the project variant uses the project's AvatarSeed and derives live state from the project's most-recently-active conversation (falling back to a neutral default if the project has no conversations). Keeping this behaviour behind a single service keeps controllers and callers decoupled from repository and generator details and ensures consistent visual identity and state-aggregation semantics.

## Example
```csharp
// Resolve the service (DI) and request a sequence for a conversation
var sequence = await gabrielSequenceService.GetForConversationAsync(conversationId, cancellationToken);
// Render or return the sequence to the client
return Ok(sequence);

// Request the shared project-level sequence (uses project's AvatarSeed)
var projectSequence = await gabrielSequenceService.GetForProjectAsync(projectId, cancellationToken);
```

## Notes
- GetForProjectAsync uses the project's AvatarSeed and aggregates live state from the project's most-recently-active conversation; if the project has no conversations yet, the sequence is rendered against a default neutral state.
- Both methods are asynchronous and accept a CancellationToken; callers should propagate cancellation where appropriate.