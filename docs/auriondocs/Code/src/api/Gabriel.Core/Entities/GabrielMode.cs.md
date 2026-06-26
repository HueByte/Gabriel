# GabrielMode

> **File:** `src/api/Gabriel.Core/Entities/GabrielMode.cs`  
> **Kind:** enum

A set of discrete persona "modes" that can be attached to a Conversation to influence per-turn system prompts. Use this enum when you want the system prompt to bias the assistant's style (length, level of explanation, stance, etc.) without rewriting the entire persona; the chosen mode selects a Fragments.Mode* snippet that GabrielSystemPromptBuilder splices into each turn.

## Remarks
GabrielMode values are stored as an integer on Conversation.Mode (nullable — null means Chatty/default). The enum does not itself perform prompt construction; it is a lightweight signal used by PromptRegistry/PromptKey and GabrielSystemPromptBuilder to pick which Fragments.Mode* snippet to include. This lets callers switch high-level behavior (e.g., Tutor vs. Concise) without touching prompt-building logic.

## Example
```csharp
// Set a conversation to the Tutor mode
conversation.Mode = GabrielMode.Tutor;

// Read with a null fallback to the Chatty default
var mode = conversation.Mode ?? GabrielMode.Chatty;
switch (mode)
{
    case GabrielMode.Tutor:
        // behavior that relies on the Tutor fragment being applied
        break;
    case GabrielMode.Concise:
        // behavior for concise responses
        break;
    // ...
}
```

## Notes
- The enum values are persisted as integers; do not reorder or renumber existing members because that will change the meaning of stored data.
- Adding a new mode requires coordinated updates: a Fragments.Mode* constant, a PromptKey entry and PromptRegistry mapping, and a new case in GabrielSystemPromptBuilder that maps the mode to the PromptKey.
- Null on Conversation.Mode is treated as GabrielMode.Chatty; callers that read Mode should handle the nullable case explicitly if they need a non-default behavior.