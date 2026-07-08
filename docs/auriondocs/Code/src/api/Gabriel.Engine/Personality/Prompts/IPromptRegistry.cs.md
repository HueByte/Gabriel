# IPromptRegistry

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/IPromptRegistry.cs`  
> **Kind:** interface

```csharp
public interface IPromptRegistry
```


IPromptRegistry exposes a read-only contract for retrieving named prompt fragments that form the system prompt. It abstracts how prompt content is stored so callers can fetch fragments by key without coupling to a particular storage strategy. Today, the fragments live as const strings in Fragments.* partials, but the interface is intentionally storage-agnostic to accommodate future backends (embedded resources, external markdown, etc.) without forcing changes at call sites. Retrieved fragments may contain placeholder tokens (e.g. {name}); substitution is the caller's responsibility, keeping the registry free of rendering logic.

## Remarks
This interface serves as a lightweight registry abstraction that decouples prompt data from the code that consumes it. By offering a single Get method, it standardizes retrieval while allowing the backing store to evolve—from in-code constants to alternative storage backends—without altering consumers. Token substitution is intentionally left to the caller, promoting a simple, predictable usage model and enabling flexible prompt assembly at consumption time.

## Example
```csharp
// Example usage of IPromptRegistry
string raw = registry.Get("PersonaFewShot");
string prompt = raw.Replace("{name}", "Ada");
```

## Notes
- Missing keys: The contract does not specify behavior for absent keys; callers should handle potential null or empty results.
- Read-only contract: The interface exposes only retrieval; mutation, if needed, would be handled by the backing storage implementation or a separate layer.
