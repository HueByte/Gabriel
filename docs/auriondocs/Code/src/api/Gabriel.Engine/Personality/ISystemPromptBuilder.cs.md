# ISystemPromptBuilder

> **File:** `src/api/Gabriel.Engine/Personality/ISystemPromptBuilder.cs`  
> **Kind:** interface

Builds the per-turn system prompt by composing a static persona block, a mode-specific bias (from Fragments), and any dynamic guidance derived from the provided ConversationState. Use this interface when you need a single, consistent place to produce the system prompt string that will be sent to the model for each turn.

## Remarks
This interface represents a stateless service responsible for assembling the system prompt for one turn. Implementations splice together three concerns: a fixed persona description, a small per-mode fragment (selected via GabrielMode), and dynamic instructions or context extracted from ConversationState. The caller provides the current conversation state and an optional mode; null mode is treated as GabrielMode.Chatty (the baseline behavior).

## Example
```csharp
// Assume an ISystemPromptBuilder is registered in DI and injected.
ConversationState? state = GetCurrentConversationState();
GabrielMode? mode = GabrielMode.Concise; // or null to use Chatty
string systemPrompt = systemPromptBuilder.Build(state, mode);
// systemPrompt can now be sent as the assistant's system message when calling the model
```

## Notes
- ConversationState is nullable; implementations should handle a null state gracefully.
- A null mode is interpreted as GabrielMode.Chatty by convention; callers can pass null to use the baseline behaviour.
- The interface is documented as a "stateless service" — implementations should be reentrant and avoid per-instance mutable state if they are to be used concurrently.
- This API only assembles the prompt text; callers remain responsible for any model-specific constraints such as token limits or additional message formatting.