# IPromptRegistry

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/IPromptRegistry.cs`  
> **Kind:** interface

Provides read-only access to named prompt fragments that are assembled into the system prompt. Use this abstraction when callers need to retrieve prompt text by key but the backing storage may vary (compile-time constants, embedded markdown resources, external files, etc.).

## Remarks
This interface decouples consumers from how prompt fragments are stored and loaded so the storage strategy can evolve without changing call sites. Fragments are returned verbatim; they may contain placeholder tokens (for example, "{name}") and the registry does not perform substitution — the caller is responsible for any token replacement or parameterization.

## Example
```csharp
// Obtain an implementation of IPromptRegistry from your DI container or factory
IPromptRegistry registry = serviceProvider.GetRequiredService<IPromptRegistry>();

// Retrieve a named fragment (may contain placeholders like "{name}")
string template = registry.Get("welcome_message");

// Perform caller-side substitution
string prompt = template.Replace("{name}", userName);
// or with more advanced templating as needed
```

## Notes
- Implementations may choose different behaviors for missing keys (return null, throw, or return a sentinel); callers should handle a null or unexpected value.
- The registry returns raw fragments; do not expect parameter substitution, trimming, or localization to be applied automatically.
- Backing stores could be cheap (const strings) or involve I/O (file/resource loading); callers should not assume access is free and may wish to cache results if performance matters.
