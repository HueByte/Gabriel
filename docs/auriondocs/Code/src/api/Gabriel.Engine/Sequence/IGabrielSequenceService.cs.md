# IGabrielSequenceService

> **File:** `src/api/Gabriel.Engine/Sequence/IGabrielSequenceService.cs`  
> **Kind:** interface

```csharp
public interface IGabrielSequenceService
```


IGabrielSequenceService provides asynchronous access to GabrielSequence data tailored to a specific user context by assembling AvatarSeed and ConversationState and delegating to the generator. Use GetForConversationAsync to fetch a sequence for a single conversation, or GetForProjectAsync to fetch one for a project that shares an AvatarSeed and aggregates Live State from the project's most recently active conversation (defaulting to a neutral state if none exist).

## Remarks
This interface acts as a stable bridge between the UI/controllers and the data/generator layers. It encapsulates how avatar identity (AvatarSeed) and conversational state are composed into a GabrielSequence, so callers don't need repository or generator details. For projects, it guarantees a consistent visual identity by sharing the AvatarSeed across conversations and by deriving live frames from the project's most recently active conversation, falling back to a neutral state when no activity exists.