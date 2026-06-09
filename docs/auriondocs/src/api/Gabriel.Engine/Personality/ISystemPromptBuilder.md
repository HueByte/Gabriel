# ISystemPromptBuilder

> **File:** `src/api/Gabriel.Engine/Personality/ISystemPromptBuilder.cs`  
> **Kind:** interface

Constructs the per-turn system prompt used by the model by combining three pieces: a static persona block, a mode-specific bias fragment, and dynamic guidance derived from the provided ConversationState. Use this interface when you need a single, consistent place to build the full system prompt string for each turn instead of assembling fragments manually.

## Remarks
This abstraction centralizes prompt assembly so callers don't need to know how fragments are selected or how ConversationState influences dynamic guidance. Implementations are intended to be stateless (suitable for registration as a singleton); they pick the appropriate Fragments.Mode* snippet for the requested mode and splice in state-derived guidance when a ConversationState is provided. Per the contract, a null mode is treated as GabrielMode.Chatty and ConversationState may be null, in which case only the persona and mode bias are used.

## Example
```csharp
// typical usage in a handler or service
public class TurnProcessor
{
    private readonly ISystemPromptBuilder _promptBuilder;

    public TurnProcessor(ISystemPromptBuilder promptBuilder)
    {
        _promptBuilder = promptBuilder;
    }

    public void ProcessTurn(ConversationState? state)
    {
        // null mode is handled by the builder as GabrielMode.Chatty
        string systemPrompt = _promptBuilder.Build(state, mode: null);
        // send systemPrompt to model with the user's message
    }
}
```

## Notes
- Null mode is treated as GabrielMode.Chatty — callers can pass null to get the baseline behaviour.  
- ConversationState may be null; implementations should handle that by omitting dynamic guidance.  
- Implementations should remain stateless; if you add caching or mutable fields, ensure proper synchronization for concurrent use.