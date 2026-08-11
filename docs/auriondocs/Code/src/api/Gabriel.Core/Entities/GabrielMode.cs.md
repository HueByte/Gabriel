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


GabrielMode is an enum that represents the behavioral bias applied to a single Conversation. It determines which Fragments.Mode* snippet is spliced into the per-turn system prompt, effectively reweighting the persona without rewriting it. The value is stored on Conversation.Mode as a nullable int, with null meaning the default Chatty mode.

## Remarks
GabrielMode serves as a central, typed flag that coordinates how the system builds prompts for a conversation. By isolating the persona mode in this enum, the system can swap between modes (Chatty, Elaborative, Concise, Tutor, Critic) without altering the underlying prompt construction logic. The mapping from GabrielMode to the actual prompt fragments and keys is kept in the GabrielSystemPromptBuilder and related constants, so adding a new mode requires updating the builder, the Fragment set, and the prompt keys in concert.

## Notes
- The numeric values must align with the Fragment and PromptKey mappings; a mismatch can yield incorrect prompt fragments.
- Null on Conversation.Mode yields Chatty default; explicit values override that.
- Adding a new mode requires coordinated edits across Gabriel.Core, Fragments, PromptKey, and GabrielSystemPromptBuilder.