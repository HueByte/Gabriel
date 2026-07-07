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


GabrielMode encodes the behavioral bias applied to a Conversation by selecting which Fragments.Mode* snippet is spliced into the per-turn system prompt, effectively re-weighting the persona without rewriting the underlying prompt logic. It is stored as an int on Conversation.Mode (nullable; null means the default Chatty mode).

## Remarks
GabrielMode acts as a centralized switch that shapes the system prompt construction by mapping a Conversation's mode to the corresponding fragment and prompt-key combination. It ties the Conversation state to GabrielSystemPromptBuilder via ModeKey and to the persona fragments defined in Fragments and PromptKey, ensuring the intended persona flavor is applied without altering the core prompt assembly. When GabrielSystemPromptBuilder builds the final prompt, it uses the mode to determine which key and fragment to substitute.

## Notes
- Adding a new mode requires coordinated edits across multiple components: the GabrielMode value itself, a matching Fragments.Mode* constant plus a corresponding PromptKey.Mode* constant, and an updated case in GabrielSystemPromptBuilder's mode-to-key switch.
- The enum is stored as an int on Conversation.Mode; null yields Chatty as the default, so explicit mode values are only meaningful when set.