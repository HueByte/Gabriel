# ISystemPromptBuilder

> **File:** `src/api/Gabriel.Engine/Personality/ISystemPromptBuilder.cs`  
> **Kind:** interface

```csharp
public interface ISystemPromptBuilder
```


Builds the complete per-turn system prompt by combining a static persona block, a mode-specific bias fragment, and dynamic guidance derived from the current ConversationState. It is modeled as a stateless service: implementors should compute and return the prompt on demand without mutating internal state. The mode parameter selects which fragment from Fragments is spliced into the prompt; when mode is null, GabrielMode.Chatty is used as the baseline behavior.

## Remarks
By centralizing system-prompt construction behind ISystemPromptBuilder, the engine ensures consistent onboarding for different Gabriel modes while still adapting to runtime context. It decouples the prompt assembly from the caller, enabling easier testing and replacement of the prompt strategy. The collaboration with ConversationState, Fragments, and GabrielMode defines a clean contract: state drives dynamic guidance, mode selects bias, and the static persona anchors tone.

## Notes
- Null mode is treated as GabrielMode.Chatty; callers relying on a default should either omit mode or pass null.
- The interface is stateless; any per-turn variation must come from the provided ConversationState and mode argument.
- Changes in Fragments or GabrielMode may alter the produced prompts; callers should design for potential prompt evolution and avoid brittle assumptions about exact wording.