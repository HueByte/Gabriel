# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Modes.cs`  
> **Kind:** class

Holds per-mode prompt fragments that bias the assistant's behaviour for a single turn. Each constant is a short block appended to the static persona prompt based on Conversation.Mode; use these when you want to nudge length, depth, or stance (for example: more elaborate answers, very concise replies, or a tutoring tone) without replacing the base persona.

## Remarks
These constants are not a replacement for the core persona — they are lightweight, per-turn biases that get spliced into the prompt so the model re-weights its responses (length, detail, stance) while the baseline persona still governs overall tone and safety. The file includes guidance for adding more modes: keep each fragment short, describe a bias (not a full rewrite), and address both TASK and CHAT behaviours so the mode applies sensibly in both contexts.

## Example
```csharp
// Retrieve the mode fragment and append it when building the prompt
string prompt = personaBlock; // existing static persona
prompt += "\n\n" + Fragments.ModeElaborative; // append chosen mode bias
// send prompt to model
```

## Notes
- ModeChatty is intentionally minimal so the base persona drives behaviour; other modes explicitly bias that baseline.  
- Adding large mode blocks can dilute the persona and increase prompt length — prefer concise, targeted biases.  
- Each fragment uses a raw string literal; preserve formatting when editing to avoid accidental prompt corruption.