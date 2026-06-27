# GabrielSystemPromptBuilder

> **File:** `src/api/Gabriel.Engine/Personality/GabrielSystemPromptBuilder.cs`  
> **Kind:** class

Builds the full system prompt used to drive the "Gabriel" persona by combining pre-cached persona fragments, formatting rules, a per-conversation mode snippet, runtime conversation metadata and short guidance/few-shot examples. Reach for this when you need the canonical system message to send to the model for a specific turn (instead of assembling fragments manually) so the prompt structure and ordering remain consistent across turns and modes.

## Remarks
GabrielSystemPromptBuilder centralizes how the persona is presented to the model: static identity text and few-shot fragments are pre-substituted and cached in the constructor to avoid repeated string replacements, while per-turn information (mode, conversation state, dynamic guidance) is appended at Build time. The builder intentionally keeps formatting between the static persona and the mode snippet so presentation concerns are separated from identity and behaviour rules. It implements ISystemPromptBuilder and is intended to be used by the conversation orchestration layer when constructing the system message for each LLM request.

## Example
```csharp
// Typical usage inside a request pipeline
var builder = new GabrielSystemPromptBuilder(options, promptRegistry);
string systemMessage = builder.Build(conversationState, GabrielMode.Concise);
// send `systemMessage` as the system prompt to the model
```

## Notes
- Build accepts a null ConversationState; when state is null the builder emits sensible defaults (turn 0, neutral mood, zero token counts).
- The name substitution for persona fragments is performed once in the constructor and cached. Changes to the provided PersonalityOptions after construction will not be reflected in the builder's output.
- The class is effectively safe for concurrent Build calls because its instance fields are readonly and Build uses a local StringBuilder, but callers should ensure the provided IPromptRegistry and ConversationState are safe to use concurrently if used from multiple threads.