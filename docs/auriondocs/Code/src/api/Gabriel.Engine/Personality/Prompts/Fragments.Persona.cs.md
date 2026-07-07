# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Persona.cs`  
> **Kind:** class

```csharp
public static partial class Fragments
```


Fragments is a static partial class that provides the base identity seed used to construct Gabriel's system prompts. The key member, PersonaStatic, is a raw, multi-line string literal containing the identity and operating guidelines that are prepended before every reply. The {name} token inside this string is substituted at runtime by GabrielSystemPromptBuilder from PersonalityOptions.Name, enabling per-project personas without touching the code. Memory guidance is intentionally kept separate in Fragments.PersonaMemory and may be omitted when memory tools aren’t registered. This base block ends with a "Hard prohibitions" marker so per-turn fragments (e.g., mode or memory blocks) can append cleanly without disturbing the foundational persona.

## Remarks

Fragments uses a static partial class to provide a stable, reusable base persona while allowing project-specific customization and extension. By decoupling the identity (PersonaStatic) from memory and mode concerns, it supports a clean layering model: the base identity is defined here, while per-turn behavior and optional memory augmentation are composed at runtime.

## Example

```csharp
// Example: apply the {name} substitution to build a usable system prompt
var options = new PersonalityOptions { Name = "Gabriel" };
string prompt = Fragments.PersonaStatic.Replace("{name}", options.Name);
```

## Notes

- The {name} placeholder is intended to be substituted at runtime; do not remove or alter it unless you intend to override the substitution mechanism.
- PersonaStatic is defined as a raw string literal; its formatting is preserved, including line breaks and embedded quotes.
- The class is static and partial, signaling that the complete set of prompts can be extended across multiple files/fragments; per-turn fragments are appended to this base, not replace it.