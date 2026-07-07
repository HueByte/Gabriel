# ISystemPromptBuilder

> **File:** `src/api/Gabriel.Engine/Personality/ISystemPromptBuilder.cs`  
> **Kind:** interface

```csharp
public interface ISystemPromptBuilder
```


Build assembles the per-turn system prompt by combining a static persona block, a mode-specific bias, and dynamic guidance derived from the current ConversationState. It returns a ready-to-use prompt string for the given turn, selecting the appropriate Fragments.Mode* snippet based on the supplied GabrielMode; if mode is null, GabrielMode.Chatty is used as the baseline behavior.

## Remarks

By exposing a single interface for prompt construction, this abstraction decouples the caller from the specifics of how the system prompt is assembled. Implementations can vary the composition strategy (e.g., different mode biases or additional dynamic hints) without changing consumers, enabling easy testing and experimentation. The stateless contract also makes the method thread-safe and predictable in concurrent scenarios.

## Notes

- This interface is stateless; avoid internal state or caches that depend on a particular call sequence.
- Null mode maps to GabrielMode.Chatty; ensure a consistent fallback to the baseline behavior.
- Be mindful of including sensitive ConversationState data in the produced prompt; avoid leaking secrets via logs or prompt composition.