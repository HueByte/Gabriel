# PromptRegistry

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/PromptRegistry.cs`  
> **Kind:** class

```csharp
public sealed class PromptRegistry : IPromptRegistry
```


PromptRegistry is a sealed class that implements IPromptRegistry and acts as the centralized lookup for prompt fragments. It constructs a read-only dictionary mapping each PromptKey to its corresponding Fragments.* string, establishing a single source of truth used when assembling prompts. Use Get with a key to retrieve the fragment; if the key isn't registered, a KeyNotFoundException is thrown with guidance to wire the missing key in the constructor.

## Remarks
By decoupling keys from literal text, the registry makes it easy to evolve prompts without scattering strings across call sites. It enforces a strict one-to-one mapping between a PromptKey and a Fragment, ensuring consistent prompt assembly across personas and modes. The three edits described in the inline comments—adding a Fragments.* constant, adding a PromptKey.*, and wiring the mapping—are the intended extension pattern.

## Example
```csharp
var registry = new PromptRegistry();
string fragment = registry.Get(PromptKey.PersonaMemory);
```

## Notes
- Access is via exact keys; missing entries throw KeyNotFoundException with guidance to register it.
- The dictionary uses StringComparer.Ordinal, so keys are case-sensitive.
- To add a new prompt, follow the three-step process described in the code comments (add a Fragments.* constant, add a PromptKey.* constant, and wire the mapping in the constructor).
