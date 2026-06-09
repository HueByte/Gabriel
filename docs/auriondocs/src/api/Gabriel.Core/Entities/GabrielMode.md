# GabrielMode

> **File:** `src/api/Gabriel.Core/Entities/GabrielMode.cs`  
> **Kind:** enum

Represents the response-style (persona) applied to a single Conversation. Set this value on Conversation.Mode when you want the system prompt builder to splice a corresponding Fragments.Mode* snippet (e.g., make replies concise, elaborate, tutor-style, or critical) rather than rewriting the entire persona.

## Remarks
This enum drives which "Fragments.Mode*" fragment is injected into the per-turn system prompt by GabrielSystemPromptBuilder, effectively re-weighting the assistant's persona for that conversation. The value is persisted as an int on Conversation.Mode (nullable); a null value is treated as Chatty (the default). Adding or changing modes requires coordinated updates to the fragment constants, PromptKey mappings, and the prompt-building switch so runtime and persisted values remain consistent.

## Example
```csharp
// Set a conversation to tutor-style responses
conversation.Mode = (int)GabrielMode.Tutor;

// Read back (null => Chatty)
var mode = conversation.Mode.HasValue ? (GabrielMode)conversation.Mode.Value : GabrielMode.Chatty;
if (mode == GabrielMode.Tutor)
{
    // builder will include Fragments.ModeTutor when constructing the system prompt
}
```

## Notes
- Conversation.Mode is stored as a nullable int; null means Chatty. Be explicit when reading/writing to avoid unexpected defaults.
- Numeric values are persisted; do not reorder or renumber existing enum members — add new values instead to avoid data/behavior mismatches.
- Adding a new mode requires three coordinated edits: a new enum value here, the corresponding Fragments.Mode* and PromptKey constants (and PromptRegistry mapping), and a new case in the mode→PromptKey switch inside GabrielSystemPromptBuilder.