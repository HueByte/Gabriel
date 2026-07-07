# GabrielSystemPromptBuilder

> **File:** `src/api/Gabriel.Engine/Personality/GabrielSystemPromptBuilder.cs`  
> **Kind:** class

```csharp
public sealed class GabrielSystemPromptBuilder : ISystemPromptBuilder
```


GabrielSystemPromptBuilder is a sealed component that assembles the system prompt Gabriel passes to the model by stitching together a precomputed persona block, a formatting scaffold, and a per-mode snippet, then appends per-conversation metadata and guidance. It acts as the orchestrator between the Prompt Registry and per-turn ConversationState, producing a complete, ready-to-send system prompt for each turn.

## Remarks
This class centralizes how the Gabriel persona is delivered: it pulls static persona fragments and mode-specific rules from a registry, substitutes the configured name, and then layers in a per-conversation mode and dynamic state. By caching the substituted blocks (_staticBlock, _formattingBlock, _fewShotBlock), it avoids repeated string processing, ensuring prompt assembly stays performant as conversations flow. The Build method guarantees a stable prompt structure while remaining sensitive to mode (e.g., Elaborative, Concise, or default Chatty) and to simple state signals like mood, last user input length, or user stylistic cues, which the model can mirror or adapt to.

## Notes
- The Build method gracefully handles a null ConversationState by using defaults (Turn 0, Mood Neutral); callers should pass a non-null state when they want precise per-turn metadata.
- The mode is defaulted to Chatty when mode is null, ensuring a consistent prompt shape across calls; if you rely on other modes, supply an explicit GabrielMode.
- The prompt content depends on the availability and contents of IPromptRegistry; changes to the underlying prompts will affect all future prompts, so ensure registry contents align with the desired persona.