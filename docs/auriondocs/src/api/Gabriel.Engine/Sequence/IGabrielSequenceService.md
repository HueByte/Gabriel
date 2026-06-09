# IGabrielSequenceService

> **File:** `src/api/Gabriel.Engine/Sequence/IGabrielSequenceService.cs`  
> **Kind:** interface

Builds a GabrielSequence by loading the relevant conversation- or project-scoped inputs (AvatarSeed and ConversationState) and delegating to the generator. Use this service when a controller or other caller needs a ready-to-render GabrielSequence without dealing with repository lookups, avatar seeding rules, or generator wiring.

## Remarks
This interface centralizes the logic of assembling the data required to produce a GabrielSequence so callers need not know about repositories or the generator internals. GetForConversationAsync resolves a single, user-scoped conversation (its AvatarSeed and ConversationState) to produce a sequence tied to that conversation. GetForProjectAsync uses the project's AvatarSeed so every conversation in the project shares a consistent visual identity; live state frames are derived from the project's most-recently-active conversation, and if the project has no conversations the implementation should render against a neutral default state.

## Example
```csharp
// Obtain sequence for a single conversation
GabrielSequence seq = await gabrielSequenceService.GetForConversationAsync(conversationId, ct);

// Obtain project-wide sequence (shared avatar seed, latest conversation state if available)
GabrielSequence projectSeq = await gabrielSequenceService.GetForProjectAsync(projectId, ct);
```

## Notes
- Both methods are asynchronous and may perform I/O (repository or generator work); pass a CancellationToken if you need cancellability.
- The project variant intentionally uses the project's AvatarSeed for consistent branding and derives live state from the most-recently-active conversation; callers should not assume per-conversation uniqueness when using GetForProjectAsync.
- The returned GabrielSequence represents a snapshot at call time; callers that need continuous or streaming updates should implement their own polling/refreshing logic or use a different mechanism.