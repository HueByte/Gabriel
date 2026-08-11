# PromptRegistry

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/PromptRegistry.cs`  
> **Kind:** class

```csharp
public sealed class PromptRegistry : IPromptRegistry
```


PromptRegistry serves as the default mapping from PromptKey values to their corresponding prompt fragments. It constructs a single dictionary at construction time that wires each key to the exact Fragments.* text, providing a centralized, consistent source of fragments for all prompt generation.

## Remarks
By isolating the key-to-fragment wiring, the registry decouples callers from fragment content and enables simple evolution of prompts. The three coordinated edits described in the class comments (adding a Fragments constant, a PromptKey constant, and a dictionary entry) ensure the wiring remains in sync and produce a verifiable contract enforced by the compiler.

## Example
```csharp
var registry = new PromptRegistry();
string staticFragment = registry.Get(PromptKey.PersonaStatic);
```

```csharp
string tutorFragment = registry.Get(PromptKey.ModeTutor);
```

## Notes
- The registry uses ordinal string comparison for keys, so lookups are case-sensitive and must match the exact constants.
- If Get is called with a key that isn’t registered, a KeyNotFoundException is thrown with a message guiding how to wire the fragment (via the constructor and Fragments.* constant).
