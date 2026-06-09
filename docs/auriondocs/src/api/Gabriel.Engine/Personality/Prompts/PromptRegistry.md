# PromptRegistry

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/PromptRegistry.cs`  
> **Kind:** class

Provides a centralized, read-only mapping from prompt keys (the PromptKey.* constants) to their corresponding prompt fragment strings defined on Fragments.*. Reach for this when assembling prompts so callers can request the canonical fragment by key instead of referencing raw fragment constants.

## Remarks
This class is the default IPromptRegistry implementation used to keep prompt fragments in one place and ensure consistent wiring between PromptKey identifiers and Fragments constants. The dictionary is populated once in the constructor and is intended to be registered as a singleton (e.g., in DI) so the same fragment set is reused across the application. Keys are compared using StringComparer.Ordinal and only keys present at construction time are available.

## Example
```csharp
// Direct usage
var registry = new PromptRegistry();
string fragment = registry.Get(PromptKey.ModeChatty);
Console.WriteLine(fragment);

// Typical DI registration
// services.AddSingleton<IPromptRegistry, PromptRegistry>();
```

## Notes
- Calling Get with a key that is not registered throws a KeyNotFoundException with guidance on adding the mapping.
- Keys are matched with StringComparer.Ordinal (case-sensitive); use the exact PromptKey constant values.
- The internal dictionary is immutable after construction, so the registry is safe for concurrent reads but cannot be changed at runtime.