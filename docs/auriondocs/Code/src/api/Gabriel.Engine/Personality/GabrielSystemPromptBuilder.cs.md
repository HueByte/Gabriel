# GabrielSystemPromptBuilder

> **File:** `src/api/Gabriel.Engine/Personality/GabrielSystemPromptBuilder.cs`  
> **Kind:** class

```csharp
public sealed class GabrielSystemPromptBuilder : ISystemPromptBuilder
```


GabrielSystemPromptBuilder orchestrates Gabriel's system prompt by assembling three sources from the prompt registry: a static persona block, a formatting guideline, and a per-mode snippet, then layering in per-conversation metadata. It substitutes the agent's name into the static block once and caches the results to avoid repeated substitutions on every turn. The Build method accepts an optional ConversationState and an optional GabrielMode and returns the complete string used to drive the model's system prompt for that turn. This symbol is the central point where identity, mode, and runtime context merge into a single, stable prompt that can adapt across modes without duplicating logic.

## Remarks
GabrielSystemPromptBuilder is the central prompt orchestration point that keeps the base persona constant while allowing mode- or state-driven variation. It achieves this by layering a static persona, a formatting guidance block, and a mode-specific snippet fetched from the registry; by caching the substituted static and few-shot fragments, it minimizes per-turn processing. The Build method tolerates null state and mode, defaulting to sane values, reducing caller friction; this ensures a robust prompt even in edge cases.

## Notes
- Caching of static blocks means prompts update require recreate or reinitialize when the prompt registry changes or when options change, otherwise the prompt may reflect stale data.
- The class caches _staticBlock, _formattingBlock, and _fewShotBlock at construction; changes to the underlying prompts after construction won't be reflected until a new instance is created.
- Missing registry keys (PersonaStatic, PersonaFormatting, PersonaFewShot) may yield incomplete prompts; ensure the prompt registry provides them to avoid empty segments.