# GabrielSequenceService

> **File:** `src/api/Gabriel.Engine/Sequence/GabrielSequenceService.cs`  
> **Kind:** class

```csharp
public sealed class GabrielSequenceService : IGabrielSequenceService
```


GabrielSequenceService is a domain service that builds a GabrielSequence tailored to a specific conversation or project by loading the live domain state and styling overrides, then delegating to the sequence generator. Developers reach for GetForConversationAsync or GetForProjectAsync to produce a sequence that can be rendered or consumed by UI or downstream logic.

## Remarks
GabrielSequenceService acts as an orchestrator between the domain model (Conversations and Projects) and the presentation/use of GabrielSequence data. It enforces authentication by requiring a current user and selects the appropriate aggregate state (including message history for conversations) before generating the sequence. For projects, it chooses the most recently updated conversation to drive the live state; if no conversations exist, it falls back to the generator\'s neutral defaults. The implementation intentionally separates concerns: repositories supply domain state, and the generator encapsulates the rules for translating that state into a GabrielSequence.

## Example
```csharp
// For a specific conversation
GabrielSequence seqForConversation = await gabrielService.GetForConversationAsync(conversationId, ct);

// For a project (uses the latest conversation to drive live state, if any)
GabrielSequence seqForProject = await gabrielService.GetForProjectAsync(projectId, ct);
```

## Notes
- Requires an authenticated user via ICurrentUser; otherwise an UnauthorizedAccessException is thrown.
- If a conversation or project cannot be found, a NotFoundException is thrown with the resource name and key to aid debugging.
- When a project has no conversations yet, latest is null and the generator\'s neutral defaults render the sequence.
