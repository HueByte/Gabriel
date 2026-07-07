# PromptRegistry

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/PromptRegistry.cs`  
> **Kind:** class

```csharp
public sealed class PromptRegistry : IPromptRegistry
```


PromptRegistry is the central lookup that maps a PromptKey to its corresponding prompt fragment string. Built once in the constructor as a read-only dictionary, it exposes Get(string key) to retrieve the fragment for a given key, and throws a clear KeyNotFoundException when a key isn’t registered.

## Remarks

By decoupling keys from fragment values, the registry provides a stable abstraction for selecting prompt content across personas and modes without scattering literal fragments throughout the codebase. The wiring is designed to be updated in lockstep: add a Fragments.* constant, a PromptKey.* constant, and an entry in the dictionary; the comments in the source remind developers of this coordination. After construction, the mapping is immutable, making concurrent reads safe and predictable.

## Notes

- Unknown key yields a KeyNotFoundException with guidance on registration.
- Adding new mappings requires coordinated edits to Fragments.*, PromptKey.* and the dictionary in PromptRegistry (as described by the code comments).
- The registry uses StringComparer.Ordinal, so keys are matched case-sensitively; ensure consistent definitions in PromptKey.
