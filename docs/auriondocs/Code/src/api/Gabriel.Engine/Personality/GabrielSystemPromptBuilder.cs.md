# GabrielSystemPromptBuilder

> **File:** `src/api/Gabriel.Engine/Personality/GabrielSystemPromptBuilder.cs`  
> **Kind:** class

```csharp
public sealed class GabrielSystemPromptBuilder : ISystemPromptBuilder
```


GabrielSystemPromptBuilder is a sealed class that assembles the system prompt Gabriel uses to guide its behavior in a given conversation. It orchestrates the composition by pulling three blocks from an IPromptRegistry: a static persona block (personalized with the configured name), a formatting block describing the markdown surface, and a mode-specific snippet that tunes behavior (e.g., Elaborative or Concise). The Build method then appends a per-conversation mode block, followed by a [Conversation metadata] section with turn count, last user message length in tokens, and mood, plus optional nudges derived from the current ConversationState (e.g., emoji mirroring, lowercase matching, and stall warnings when the user requests detail but continues with short messages). Finally, the prompt includes a [Guidance] section built from LengthGuidance and MoodGuidance, and concludes with a pre-fetched few-shot block. The class caches the pre-substituted blocks to avoid re-running replacements on every turn and is effectively immutable after construction, enabling safe reuse across requests.