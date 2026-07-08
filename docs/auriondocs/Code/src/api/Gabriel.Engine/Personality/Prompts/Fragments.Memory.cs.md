# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Memory.cs`  
> **Kind:** class

```csharp
public static partial class Fragments
```


Fragments.PersonaMemory is a publicly accessible string constant that contains the memory-system guidance appended after the static persona block, so the model treats it as an independent capability rather than identity. It provides a centralized, reusable policy for how memory-related information should be saved, referenced, and used by the AI.

## Remarks
Fragments is designed to decouple memory guidance from the runtime persona, enabling prompting to consistently convey memory behavior without altering identity. Exposing the guidance as a static constant in a partial class allows modular extension across the codebase while keeping the policy in a single source of truth.

## Example
```csharp
// Retrieve the memory guidance for inclusion in a system prompt
string guidance = Fragments.PersonaMemory;
```

## Notes
- The content is a verbatim multi-line raw string literal; ensure formatting remains intact when injected into prompts.
- Because PersonaMemory is a const, changing it requires recompilation of the consuming assembly.
- The string documents memory tool names (memory_save, memory_list, memory_remove) and the conditions for saving or referencing information as described in the content.