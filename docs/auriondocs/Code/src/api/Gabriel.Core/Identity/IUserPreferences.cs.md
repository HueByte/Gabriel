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


IUserPreferences exposes the per-user preferences surface used by the Gabriel core infrastructure. It provides two operations: GetAsync to read the current user's preferences and SetPreferredModelAsync to update the user's preferred provider and model — with null values clearing to configured defaults. The interface is designed to be consumed via Dependency Injection by per-request services (such as AgentService) so it can leverage the active ICurrentUser and DbContext within a single request.

## Remarks
Purpose-built to isolate user-level configuration, this interface decouples business logic from the specifics of how preferences are stored or retrieved. Implementations can hydrate defaults from config, persist changes to a DbContext-backed store, and expose a consistent API for reading and updating the active user's settings. The per-request scope ensures that preferences reflect the current user context during a single request.

## Example
```csharp
// Fetch current preferences for the active user
var prefs = await userPreferences.GetAsync(ct);

// Update the preferred model/provider for the user
await userPreferences.SetPreferredModelAsync("providerX", "modelY", ct);

// Reset to configuration defaults
await userPreferences.SetPreferredModelAsync(null, null, ct);
```

## Notes
- Pass null for provider or model to clear that field and revert to the config default.
- GetAsync returns the current user's preferences for the active request and will reflect defaults if no preferences have been set yet.

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


UserPreferences is an immutable, per-user value object that carries two optional choices: PreferredProvider and PreferredModel. It is read and written outside the JWT to avoid bloating claims; the current scope is model selection, but the record shape makes it straightforward to add additional preferences later with a single-line change.

## Remarks

- This abstraction separates per-user preferences from identity claims and token payloads, reducing token size and decoupling concerns around authentication from user configuration.
- The use of a C# record provides value-based equality and immutability, and the nullable string fields (string?) allow representing an unspecified preference while preserving explicit values when provided.
- Extending the set of preferences is additive and low-risk; future fields can be added with minimal disruption to existing consumers.

## Example

```csharp
// Typical usage with both preferences provided
var prefs = new UserPreferences("OpenAI", "gpt-4");

// Example with a missing provider, but with a model preference
var prefsPartial = new UserPreferences(null, "gpt-3.5-turbo");
```

## Notes

- Nullability semantics: null in either field means the preference is not specified; callers should handle nulls explicitly.
- Because this is a positional record, adding new fields in the future requires updating constructor call sites or introducing defaulted overloads; plan for evolution when expanding the shape.
- These preferences are outside the JWT, so ensure proper access control and secure persistence wherever they are stored.

---