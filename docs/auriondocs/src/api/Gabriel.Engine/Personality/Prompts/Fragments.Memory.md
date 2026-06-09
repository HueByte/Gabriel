# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Memory.cs`  
> **Kind:** class

Contains a single, reusable prompt fragment that instructs the assistant how and when to use the long‑term memory tools (memory_save, memory_list, memory_remove). Include this fragment when assembling a system prompt for agents that support memory so the model receives consistent, project-scoped guidance about what to store, how to name entries, and when not to save information.

## Remarks
This fragment is intended to be appended after a persona or static identity block so the model treats the content as an additional capability ("you also have these capabilities") rather than part of the assistant's identity. Prompt builders may choose to skip including this constant when memory tooling is not registered; when included it should generally be kept verbatim because the text contains formatting conventions and explicit rules the downstream model is expected to follow.

## Example
```csharp
// Combine persona guidance with the memory guidance when building the system prompt
var systemPrompt = Fragments.Persona + "\n\n" + Fragments.PersonaMemory;
promptBuilder.SetSystemMessage(systemPrompt);
```

## Notes
- Fragments.PersonaMemory is a compile-time constant; changing it requires rebuilding the assembly and may affect any prompt templates that rely on its exact wording or formatting.
- The fragment is written as guidance for models and tooling; do not embed sensitive or user-specific secrets into this constant, and avoid duplicating conflicting memory rules elsewhere in the prompt stack.
