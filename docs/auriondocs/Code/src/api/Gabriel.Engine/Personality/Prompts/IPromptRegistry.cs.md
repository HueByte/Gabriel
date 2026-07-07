# IPromptRegistry

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/IPromptRegistry.cs`  
> **Kind:** interface

```csharp
public interface IPromptRegistry
```


Provides read-only access to named prompt fragments that feed the system prompt. By exposing Get(string key), the interface decouples the storage of those fragments from the consumers that compose the system prompt. Today fragments live as const strings in Fragments.* partials, but the shape is intentionally storage-agnostic to accommodate embedded resources or external markdown files if needed later. Fragments may contain placeholders (e.g. `{name}`); the responsibility for substituting those tokens lies with the caller, keeping the registry itself parameter-free and side-effect-free.

## Remarks
Architecturally, IPromptRegistry defines a clean boundary between what prompts exist and how they are retrieved. It enables swapping storage strategies without touching prompt-using code, facilitating testing with mock registries and future extensions to resource types (embedded, file-based, or remote). By treating prompt fragments as opaque strings retrieved by key, it avoids leaking storage details into consumers.

## Notes
- Unknown-key behavior is not defined by the interface; implementations may throw, return an empty string, or provide a default. Callers should guard against missing keys and handle empty results gracefully.
- This interface is intentionally read-only; to update prompts, swap the registry implementation or underlying storage rather than performing mutations at the consumer level.