# PromptRegistry

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/PromptRegistry.cs`  
> **Kind:** class

Provides a simple, read-only mapping from prompt key strings to the corresponding prompt fragment constants defined on Fragments.*. Use this registry when callers need to resolve a runtime prompt key to the actual fragment text instead of referencing Fragments.* directly — for example, when the key comes from configuration or user input.

## Remarks
This is the project's default implementation of IPromptRegistry: the dictionary is constructed once in the constructor and kept as an IReadOnlyDictionary for fast, thread-safe reads. The registry intentionally wires concrete PromptKey.* strings to the matching Fragments.* constants so the compiler enforces the relationship; adding a new fragment requires three coordinated edits (add the Fragments const, add the PromptKey constant, and add the mapping here).

## Example
```csharp
var registry = new PromptRegistry();
string fragment = registry.Get(PromptKey.PersonaStatic);
// use fragment as part of a prompt construction
```

## Notes
- Get throws KeyNotFoundException if the requested key is not present; callers should handle or validate keys beforehand.
- The internal dictionary uses StringComparer.Ordinal for key comparisons; keys are case-sensitive.
- The mapping is fixed at construction time; to add or change mappings update the Fragments and PromptKey constants and the registry constructor mapping.