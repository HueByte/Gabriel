# PromptKey

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/PromptKey.cs`  
> **Kind:** class

```csharp
public static class PromptKey
```


PromptKey is a static container of string constants that serve as named keys into the prompt fragment registry. Each key identifies a specific fragment group (persona, memory, formatting, or mode behavior) and is declared as a const string to ensure they fold into switch arms or dictionary lookups at compile time, catching typos as build errors. The keys are organized by topic using dot-separated identifiers, and whenever a new mode or section is added, its key is defined here alongside the corresponding Fragments.* member.

## Remarks
By centralizing these keys, callers avoid scattering literal strings across code and gain compile-time validation. The abstraction also makes it straightforward to extend the prompt system: add a new mode by introducing a PromptKey constant and pairing it with a Fragments.* entry, without touching the lookup logic elsewhere. It helps keep prompt assembly modular: the consumer builds prompts by combining named fragments based on the current persona and mode.

## Notes
- Ensure the key value matches a corresponding Fragments.* fragment; otherwise prompt assembly may skip or fail at runtime.
- Do not rename keys without updating call sites that rely on exact strings.
- Keep the dot-separated namespace stable; it encodes grouping and mode selection.