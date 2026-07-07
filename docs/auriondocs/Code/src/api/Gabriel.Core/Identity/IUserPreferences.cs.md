# IUserPreferences.cs

> **Source:** `src/api/Gabriel.Core/Identity/IUserPreferences.cs`

## Contents

- [IUserPreferences](#iuserpreferences)
- [UserPreferences](#userpreferences)

---

## IUserPreferences
> **File:** `src/api/Gabriel.Core/Identity/IUserPreferences.cs`  
> **Kind:** interface

```csharp
public interface IUserPreferences
```


IUserPreferences provides access to per-user customization settings for the system's AI model usage. Use GetAsync to read the current user's preferences at the start of a request, and SetPreferredModelAsync to apply or reset the user's chosen provider and model. It is implemented by Gabriel.Infrastructure and consumed via DI by services like AgentService; its per-request scope allows it to align with the active ICurrentUser and DbContext.

## Remarks
By abstracting persistence behind this interface, domain services can tailor responses to a specific user without depending on storage details. It collaborates with the current user context and the DbContext to ensure that changes apply to the correct user and scope. The ability to pass null for both parameters in SetPreferredModelAsync cleanly reverts to the default configuration when a user wants to drop customizations.

## Example
```csharp
// Example usage
public class ExampleUsage
{
    private readonly IUserPreferences _prefs;
    public ExampleUsage(IUserPreferences prefs) { _prefs = prefs; }

    public async Task RunAsync(CancellationToken ct)
    {
        var current = await _prefs.GetAsync(ct);
        // React to current preferences as needed

        await _prefs.SetPreferredModelAsync("providerA", "model123", ct);

        // Revert to application defaults
        await _prefs.SetPreferredModelAsync(null, null, ct);
    }
}
```

## Notes
- Pass the CancellationToken through to support operation cancellation. 
- GetAsync returns a UserPreferences instance; changes should be persisted via SetPreferredModelAsync rather than mutating a returned object. 
- This interface is scoped per-request; avoid caching preferences across requests to prevent context leakage.

---

## UserPreferences
> **File:** `src/api/Gabriel.Core/Identity/IUserPreferences.cs`  
> **Kind:** record

```csharp
public sealed record UserPreferences(string? PreferredProvider, string? PreferredModel)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `PreferredProvider` | `string?` | — |
| `PreferredModel` | `string?` | — |


Represents a small per-user value object that captures the user's preferred provider and model. It is intentionally kept outside of JWT claims to avoid bloating tokens, and its structure is a record to support straightforward evolution—adding fields is a one-line change. The two fields, PreferredProvider and PreferredModel, are nullable, reflecting cases where a user has not expressed a preference for one or both dimensions. As a sealed positional record, it offers value-based equality and immutability, while benefiting from deconstruction and with-expressions for convenient copy-on-write updates.

## Remarks

By isolating user preferences from authentication data, this type decouples concerns and enables consistent propagation of user choices across layers (UI, API, and domain logic). It provides a stable, contract-friendly carrier that can be serialized or transferred without inflating JWTs, while still allowing future enhancements via the same shape.

## Notes

- Null values indicate unspecified preferences; callers must handle nulls or supply defaults.
- Record semantics provide value-based equality and immutability; use with to derive modified copies rather than mutating instances.
- Be mindful of contract evolution: changing property names or types affects serialization and consumers; coordinate changes.

---