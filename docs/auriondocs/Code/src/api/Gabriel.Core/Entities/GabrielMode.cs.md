# GabrielMode

> **File:** `src/api/Gabriel.Core/Entities/GabrielMode.cs`  
> **Kind:** enum

```csharp
public enum GabrielMode
{
    Chatty      = 0,
    Elaborative = 1,
    Concise     = 2,
    Tutor       = 3,
    Critic      = 4,
}
```


GabrielMode is an enum that encodes the behavioral bias attached to a single Conversation. It drives which Fragments.Mode* snippet gets spliced into the per-turn system prompt, effectively re-weighting the persona without rewriting it. The mode is stored as an int on Conversation.Mode (nullable; null = Chatty default). Adding a new mode requires three coordinated edits: a new enum value here, a corresponding Fragments.Mode* constant plus a PromptKey.Mode* constant plus a PromptRegistry mapping, and a new case in the mode→PromptKey switch in GabrielSystemPromptBuilder.

## Remarks
The enum centralizes persona variants and decouples their selection from the prompt-building pipeline. It acts as a single source of truth for mode-based prompt fragments and, together with GabrielSystemPromptBuilder, enables consistent behavior changes across conversations without scattering logic across multiple files.

## Notes
- Be mindful that the selected mode is stored as an int on Conversation.Mode; if you serialize or migrate data, ensure compatibility with existing values and the default Chatty behavior when null is encountered.
- When introducing a new mode, you must update all three coordinated locations exactly as described above; missing any step can cause the system to ignore the new mode or fail to map it to the correct prompt fragments.