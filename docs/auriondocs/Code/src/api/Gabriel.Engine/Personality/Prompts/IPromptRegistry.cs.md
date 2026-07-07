# IPromptRegistry

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/IPromptRegistry.cs`  
> **Kind:** interface

```csharp
public interface IPromptRegistry
```


IPromptRegistry provides read-only access to named prompt fragments and decouples prompt storage from consumers. Use Get to fetch a template by key at runtime, rather than embedding strings or coupling to a specific Fragments implementation.

## Remarks

By layering prompt access behind this minimal interface, the system can swap storage strategies (in-code constants, embedded resources, or external sources) without changing caller code. The interface also makes explicit that the registry supplies templates that may contain placeholders (e.g. `{name}`), leaving the substitution step to the caller. This separation helps keep concerns tidy and improves testability by allowing mock implementations.

## Example

```csharp
// Example usage: retrieve a template and substitute tokens
IPromptRegistry registry = someRegistryInstance; // implementation provided at runtime
string template = registry.Get("PersonaFewShot");
string result = template.Replace("{name}", "Ada");
```

## Notes

- The contract doesn't define behavior for missing keys; handle null/empty results accordingly.
- Placeholders like `{name}` are not substituted automatically; you must perform replacement.