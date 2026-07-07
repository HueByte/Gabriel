# IGabrielSequenceService

> **File:** `src/api/Gabriel.Engine/Sequence/IGabrielSequenceService.cs`  
> **Kind:** interface

```csharp
public interface IGabrielSequenceService
```


The IGabrielSequenceService interface provides a single integration point for obtaining GabrielSequence objects used to render conversation visuals. It abstracts away the details of repository access and generator orchestration by loading the appropriate AvatarSeed and ConversationState before handing them to the sequence generator. The two methods express the different scoping guarantees: GetForConversationAsync returns a sequence tailored to one specific conversation, while GetForProjectAsync uses the project's AvatarSeed and aggregates Live State from the project-wide latest active conversation (falling back to a neutral state if no conversations exist). Controllers should depend on this interface rather than directly interacting with repositories or the generator to keep concerns well separated and ensure consistent sequence rendering across UI layers. 

## Remarks

This abstraction centralizes the logic for selecting identity (AvatarSeed) and state (ConversationState or Live State) and routes both per-conversation and per-project sequencing through a single surface. By offering both scopes, it enables consistent project-wide visuals while preserving per-conversation customization when needed. It also isolates the UI/API boundary from the details of how sequences are sourced and produced, making future changes to data access or the generator transparent to callers.

## Example
```csharp
// Common usage
GabrielSequence seqForConv = await gabrielSequenceService.GetForConversationAsync(conversationId, cancellationToken);
GabrielSequence seqForProj = await gabrielSequenceService.GetForProjectAsync(projectId, cancellationToken);
```

## Notes
- Ensure the provided CancellationToken is observed by the caller; the implementation respects the token and may cancel pending work.
- The per-project variant relies on the project's AvatarSeed and the most recently active conversation for Live State; if no conversations exist yet, rendering falls back to a neutral state, which may affect initial visuals until activity is recorded.