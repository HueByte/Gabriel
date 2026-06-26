# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Memory.cs`  
> **Kind:** class

Holds a reusable prompt fragment that documents how the assistant should use long-term memory tools and when those tools should be used. Reach for this constant when assembling system/persona prompts or packaging prompt fragments so the runtime understands the rules for saving, listing, and removing memories.

## Remarks
This partial Fragments class centralizes prompt fragments used by the persona/prompt builder; PersonaMemory is intentionally a long, multi-line constant so the same guidance can be appended to the system prompt wherever memory behavior must be specified. The fragment is designed to be appended after the static persona block (so it reads as additional capabilities) and can be skipped cleanly by builders that don't register memory tools.

## Example
```csharp
// Compose a system prompt by appending the persona then the memory guidance
var promptBuilder = new StringBuilder();
promptBuilder.Append(Fragments.Persona);          // other persona fragments live in this partial class
promptBuilder.Append("\n\n");
promptBuilder.Append(Fragments.PersonaMemory);  // adds memory tool guidance
var systemPrompt = promptBuilder.ToString();
```

## Notes
- The value is a compile-time constant; changing it alters assistant behavior across all places it's used.
- Builders may intentionally omit this fragment if no memory tools are available; the fragment text is written so callers can skip it without further changes.
- This string is not localized by default — if you need translated guidance, introduce separate localized fragments rather than editing the constant in-place.