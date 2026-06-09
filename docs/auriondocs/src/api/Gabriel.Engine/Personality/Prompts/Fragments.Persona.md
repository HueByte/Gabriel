# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Persona.cs`  
> **Kind:** class

Holds static, compile-time prompt fragments used to build the assistant's system/persona prompt. Reach for these constants when assembling or inspecting the base system prompt text (for example, in a system prompt builder) rather than embedding large literal strings throughout the codebase.

## Remarks
This partial class centralizes the immutable pieces of the persona/system prompt so they can be composed with other fragments (memory, per-turn mode text, etc.) at runtime. The PersonaStatic fragment contains the "who you are" header and is intentionally kept free of memory guidance so memory-related fragments can be appended conditionally. The class is split as partial to keep large prompt sections organized across files.

## Example
```csharp
// Replace the {name} token with the configured assistant name and append any memory fragment.
var systemPrompt = Fragments.PersonaStatic.Replace("{name}", options.Name)
                 + "\n" + Fragments.PersonaMemory; // PersonaMemory is defined in a different partial file

// Pass systemPrompt to whatever builder or prompt envelope you use.
```

## Notes
- The PersonaStatic text contains a `{name}` token that must be substituted at runtime (e.g., with PersonalityOptions.Name).
- These fragments are const strings (compile-time), so changing them requires recompilation and they are inherently thread-safe.
- PersonaStatic purposely omits memory guidance and ends with the "Hard prohibitions" section so downstream fragments can append mode- or memory-specific content in a predictable way.