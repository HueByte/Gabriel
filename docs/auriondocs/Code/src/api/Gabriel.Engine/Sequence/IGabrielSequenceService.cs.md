# IGabrielSequenceService

> **File:** `src/api/Gabriel.Engine/Sequence/IGabrielSequenceService.cs`  
> **Kind:** interface

```csharp
public interface IGabrielSequenceService
```


IGabrielSequenceService provides asynchronous access to GabrielSequence data needed for rendering sequences tied to either a single conversation or an entire project. It hides the details of data retrieval and generation behind a simple API surface: GetForConversationAsync loads the user-scoped AvatarSeed and ConversationState for a given conversation and passes them to the generator; GetForProjectAsync uses the project's AvatarSeed so all conversations in that project render with the same identity, aggregating Live State from the most recently active conversation, or falling back to a neutral default if the project has no conversations yet.

## Remarks
This interface acts as a boundary between controllers and the underlying repositories and sequence generator. By centralizing how AvatarSeed and ConversationState are loaded and handed to the generator, it reduces duplication and helps enforce a consistent visual identity across conversations within a project.

## Example
```csharp
// Obtain a sequence for a conversation
var sequence = await gabrielSequenceService.GetForConversationAsync(conversationId, cancellationToken);

// Obtain a sequence for a project-wide identity (shared AvatarSeed)
var projectSequence = await gabrielSequenceService.GetForProjectAsync(projectId, cancellationToken);
```

## Notes
- The methods are asynchronous and accept a CancellationToken to support cancellation in calling code; pass a token from the caller to enable cooperative cancellation.
- GetForProjectAsync relies on the project's latest active conversation to derive Live State; if the project has no conversations yet, a neutral default state is used.