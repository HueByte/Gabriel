# IUserPreferences.cs

> **Source:** `src/api/Gabriel.Core/Identity/IUserPreferences.cs`

## Contents

- [IUserPreferences](#iuserpreferences)
- [UserPreferences](#userpreferences)

---

## IUserPreferences

> **File:** `src/api/Gabriel.Core/Identity/IUserPreferences.cs`  
> **Kind:** interface

Provides access to the current user's persisted preferences (read and update) such as the preferred model/provider for agent calls. Use this interface when you need to read or change per-user settings via dependency injection within a request scope (for example from an AgentService or a controller).

## Remarks
This abstraction sits between higher-level services (AgentService, controllers) and the persistence layer; implementations (the project provides one in Gabriel.Infrastructure) typically use the request-scoped ICurrentUser and a DbContext to load and persist preferences for the authenticated user. The interface is intentionally minimal — read the full preferences with GetAsync and change only the model/provider pair with SetPreferredModelAsync.

## Example
```csharp
// constructor injection in a service or controller
public class AgentsController
{
    private readonly IUserPreferences _prefs;

    public AgentsController(IUserPreferences prefs)
    {
        _prefs = prefs;
    }

    public async Task UsePreferredModel(CancellationToken ct)
    {
        var prefs = await _prefs.GetAsync(ct);
        var provider = prefs.PreferredProvider; // example property on UserPreferences
        var model = prefs.PreferredModel;

        // set a new preferred model
        await _prefs.SetPreferredModelAsync("openai", "gpt-4o", ct);

        // clear preferred model/provider to fall back to config defaults
        await _prefs.SetPreferredModelAsync(null, null, ct);
    }
}
```

## Notes
- The service is intended to be registered scoped per-request; do not cache an instance across requests.
- Passing (null, null) to SetPreferredModelAsync clears the user-specific choice so the system will fall back to configuration defaults.
- Both methods are asynchronous and accept a CancellationToken; callers should propagate cancellation and expect I/O (persistence) activity under the hood.

---

## UserPreferences

> **File:** `src/api/Gabriel.Core/Identity/IUserPreferences.cs`  
> **Kind:** record

Holds per-user preferences that are persisted outside of JWT claims (to avoid enlarging tokens). It currently captures two optional choices—PreferredProvider and PreferredModel—and is intended for simple, versionable user settings such as preferred model/provider; use this record whenever you need to read, persist, or pass around those preferences as a single immutable value.

## Remarks
This type is defined as a record so additions are a one-line change and callers get value-based equality and convenient immutability helpers (for example, the with-expression). Keeping these preferences outside the JWT prevents token size growth and lets preferences evolve independently of authentication claims.

## Example
```csharp
// Create preferences
var prefs = new UserPreferences("openai", "gpt-4");

// Update immutably using a with-expression
var updated = prefs with { PreferredModel = "gpt-4o" };

// Deconstruct if needed
var (provider, model) = prefs;
```

## Notes
- Both properties are nullable; callers should handle null as "no preference set." 
- The record is immutable after construction; change values via the with-expression rather than by mutation. 
- Some serializers may require configuration to deserialize positional records (they may not call the primary constructor by default); verify your serializer or provide a converter if deserialization fails.

---