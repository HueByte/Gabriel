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


IUserPreferences is a small, per-user contract that exposes the capability to read and persist the active user's preferences related to model/provider selection. Use GetAsync to retrieve the current UserPreferences for the active context, and SetPreferredModelAsync to persist a chosen provider and model for that user. If you pass null for both provider and model, the user’s selection is cleared and the system default configured elsewhere is used instead.

Implemented by Gabriel.Infrastructure and consumed by AgentService via dependency injection, this contract is intentionally scoped per-request so it can reliably access per-request context such as ICurrentUser and DbContext without leaking infrastructure concerns into callers.

## Remarks

The abstraction decouples business logic from the persistence details of per-user configuration, centralizing how a user’s preferred model/provider is read and written. This enables per-user customization without forcing callers to know about the underlying data store or lifecycle concerns. The per-request scope also improves testability, as implementations can be stubbed or mocked in tests while preserving correct user context during real requests.

## Notes

- To reset to the configured default, call SetPreferredModelAsync(null, null, ct). Partial resets (e.g., clearing only provider or only model) are not defined by this contract.
- Because the implementation is DI-scoped per request, do not rely on caching the returned preferences across requests; fetch fresh data per request when needed.

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


UserPreferences is an immutable value object that captures a per-user choice of provider and model. It lives outside JWT claims to avoid bloating tokens, and it is currently scoped to model selection. By modeling these values as a record, adding new fields in the future is a one-line change that won't disrupt existing consumers.

## Remarks

Because it is a sealed record, it benefits from value-based equality and clear copying semantics (via with-expressions). It serves as a lightweight boundary between identity/authorization concerns and runtime model selection, enabling centralized handling of per-user preferences and easier future evolution.

## Example

```csharp
var prefs = new UserPreferences("OpenAI", "gpt-4");
```

## Notes

- Nullable fields mean absence indicates no preference; callers should provide sensible defaults when a value is null.
- Being a record, you cannot mutate an existing instance. To apply changes, create a new instance or use a with-expression to copy with modified fields (e.g., `prefs with { PreferredModel = "gpt-5" }`).

---