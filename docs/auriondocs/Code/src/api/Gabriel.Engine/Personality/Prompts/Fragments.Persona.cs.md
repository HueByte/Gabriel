# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Persona.cs`  
> **Kind:** class

```csharp
public static partial class Fragments
```


Fragments is a static container for prompt fragments used by the Gabriel Engine to assemble its system prompts. In particular, PersonaStatic stores the static persona block that defines the engine's identity, its default operating mode, and the boundaries it should observe when replying. The string is designed to be assembled at runtime by GabrielSystemPromptBuilder, which substitutes the {name} token from PersonalityOptions.Name (defaulting to 'Gabriel'), enabling project-specific personas without touching this file. Memory guidance is intentionally kept separate in Fragments.PersonaMemory and may be omitted if memory tools are not registered. The static block concludes with the phrase 'Hard prohibitions', providing a reliable anchor point for per-turn fragments to append memory/mode specifics without corrupting the base block.

## Remarks
Fragments acts as the canonical, central source of the agent's identity and behavioral rules. By isolating the static persona in Fragments, the system can swap the displayed name per project while preserving a consistent baseline. It also cleanly separates identity and memory concerns: PersonaMemory handles memory guidance, while PersonaStatic ensures a predictable header is present before every reply. This design reduces duplication and makes it easier to evolve the persona independently from per-turn prompts.

## Example
```csharp
// Example: runtime substitution of the {name} placeholder
string raw = Fragments.PersonaStatic;
string concrete = raw.Replace("{name}", "Astra");
```

## Notes
- The {name} token is substituted at runtime by the system (GabrielSystemPromptBuilder) from PersonalityOptions.Name; do not rely on the static string containing a final name during design.
- Changing the static block affects all responses; prefer adjusting memory or per-turn prompts (PersonaMemory) for dynamic behavior rather than altering PersonaStatic.
- Memory guidance is intentionally separate; if memory tools are unavailable, PersonaMemory content may not be injected, so ensure the system can operate with the base static block alone.