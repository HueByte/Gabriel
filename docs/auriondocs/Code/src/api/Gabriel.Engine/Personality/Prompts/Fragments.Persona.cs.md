# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Persona.cs`  
> **Kind:** class

Contains the static, canonical "who you are" header used when assembling the assistant's system prompt. Use Fragments.PersonaStatic to include the stable persona text in a system message; the string contains a {name} token that GabrielSystemPromptBuilder substitutes at runtime from PersonalityOptions.Name. This fragment intentionally omits memory guidance and finishes with the "Hard prohibitions" marker so per-turn or memory fragments can be appended cleanly.

## Remarks
Centralizes the unchanging portion of the system prompt so different builders and tests can reuse the same authoritative persona text. Memory-specific instructions are kept in a separate fragment so the prompt construction can include or omit them depending on whether memory tools are registered.

## Example
```csharp
// Replace the {name} token with the configured persona name and use as the system prompt.
var systemPrompt = Fragments.PersonaStatic.Replace("{name}", personalityOptions.Name);
// Pass systemPrompt into your chat/message builder as the system-level instruction.
```

## Notes
- PersonaStatic is a const raw-string literal: changing it requires recompilation of the assembly.
- The raw string preserves whitespace and newlines; check spacing when concatenating other fragments to avoid accidental blank lines.
- Memory guidance intentionally lives in a different fragment (e.g. Fragments.PersonaMemory) — include that fragment only when memory tools are available and you want memory instructions in the prompt.