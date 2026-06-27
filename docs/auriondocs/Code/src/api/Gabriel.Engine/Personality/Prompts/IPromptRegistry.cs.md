# IPromptRegistry

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/IPromptRegistry.cs`  
> **Kind:** interface

Provides read-only lookup of named prompt fragments that feed the system prompt. Use this interface when you need to retrieve a prompt template by key while keeping the storage mechanism (constants, embedded resources, or external files) decoupled from consumers; any placeholder substitution (for example `{name}`) is the caller's responsibility.

## Remarks
The interface exists to separate how prompt fragments are stored from how they are consumed so implementations can evolve without forcing changes across callers. It intentionally returns raw strings and exposes no parameters or formatting helpers — that keeps the registry simple and allows consumers to apply their preferred substitution or localization strategies.

## Example
```csharp
// Retrieve a fragment and perform simple token substitution
string fragment = promptRegistry.Get("greeting"); // e.g. "Hello, {name}!"
string rendered = fragment?.Replace("{name}", userName) ?? throw new InvalidOperationException("Missing prompt fragment: greeting");
```

## Notes
- Fragments may include placeholder tokens (e.g. `{name}`); callers must perform substitution.
- IPromptRegistry provides read-only access only; it does not expose mutation operations.
- The interface does not mandate behavior for missing keys (null vs. exception); callers should handle absent fragments according to the specific implementation in use.