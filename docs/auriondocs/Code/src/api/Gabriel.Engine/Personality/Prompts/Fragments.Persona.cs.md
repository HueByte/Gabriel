# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Persona.cs`  
> **Kind:** class

```csharp
public static partial class Fragments
```


Fragments is a static partial container for the Gabriel engine’s persona fragments. It defines the core static header used to initialize every system prompt via the PersonaStatic constant, which encodes identity and initial behavioral guidance for the assistant. The {name} placeholder is substituted at runtime by GabrielSystemPromptBuilder from PersonalityOptions.Name (default \"Gabriel\"), enabling per-project personas to swap the name without touching this file. Memory guidance is intentionally kept separate (in Fragments.PersonaMemory) and may be omitted when memory tools aren’t registered. The static block ends with the phrase \"Hard prohibitions\" so a per-turn fragment (mode / memory) can append cleanly to this base header.

## Remarks
Fragments serves as the architectural anchor for the initial system prompt, separating identity/header content from memory and per-turn behavior. By making the container static and partial, the codebase can extend or override persona-related fragments without mutating the core header, supporting flexible customization across projects while maintaining a consistent starting prompt.

## Notes
- PersonaStatic is a const string; it cannot be mutated at runtime. Customization should occur via additional fragments or by changing the runtime name used in substitution.
- The {name} token must be replaced before the header is used by the system prompt (the comments indicate this happens via GabrielSystemPromptBuilder).
- Memory guidance belongs in Fragments.PersonaMemory and is appended separately, ensuring a clean separation of concerns between identity/header content and memory/mode behavior.