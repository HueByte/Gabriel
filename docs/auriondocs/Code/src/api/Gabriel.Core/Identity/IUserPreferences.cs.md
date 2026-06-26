# IUserPreferences.cs

> **Source:** `src/api/Gabriel.Core/Identity/IUserPreferences.cs`

## Contents

- [IUserPreferences](#iuserpreferences)
- [UserPreferences](#userpreferences)

---

## IUserPreferences

> **File:** `src/api/Gabriel.Core/Identity/IUserPreferences.cs`  
> **Kind:** interface

Provides access to the current user's persisted preferences (for example, preferred provider and model) and allows updating them; consume via dependency injection when code running in the context of a request needs to read or change the active user's preferences.

## Remarks
This interface is implemented by Gabriel.Infrastructure and is registered scoped per-request so implementations can rely on ICurrentUser and the request-scoped DbContext. It exists to centralize preference read/write logic (including the semantics for clearing preferences) and to keep callers decoupled from persistence details.

## Example
```csharp
// Typical consumption in a request-scoped service
public class AgentService
{
    private readonly IUserPreferences _userPreferences;

    public AgentService(IUserPreferences userPreferences)
    {
        _userPreferences = userPreferences;
    }

    public async Task EnsureModelAsync(CancellationToken ct)
    {
        var prefs = await _userPreferences.GetAsync(ct);

        if (prefs.PreferredProvider != "openai")
        {
            // Update preferences (pass null to clear back to config defaults)
            await _userPreferences.SetPreferredModelAsync("openai", "gpt-4", ct);
        }
    }
}
```

## Notes
- Passing null for provider and/or model clears that value so the system falls back to the configured default.
- Methods are asynchronous and may perform database I/O; do not call them from constructors or long-lived singleton contexts (the implementation is scoped per-request).
- Respect the CancellationToken: operations may observe it while accessing the DbContext or external stores.

---

## UserPreferences

> **File:** `src/api/Gabriel.Core/Identity/IUserPreferences.cs`  
> **Kind:** record

Holds lightweight per-user preferences that are kept outside the JWT. Currently contains the user's preferred provider and preferred model; reach for this record when you need to persist or pass around model-selection choices without inflating JWT claims.

## Remarks
This record exists to separate transient authentication claims from user-configurable preferences (claims would grow if every preference were embedded in the JWT). It is intentionally a small, sealed record so adding fields is a one-line change and value-based equality, immutability, and with-expressions are available for easy comparisons and updates.

## Example
```csharp
// Create preferences
var prefs = new UserPreferences("openai", "gpt-4o");

// Update immutably using a with-expression
var updated = prefs with { PreferredModel = "gpt-4o-mini" };

// Deconstruct or read properties
var (provider, model) = prefs;
Console.WriteLine($"Provider={provider}, Model={model}");
```

## Notes
- Properties are nullable; treat null as "unset" and provide sensible defaults when applying preferences.
- The record is immutable (init-only); to change values create a new instance or use the with-expression shown above.
- Equality is value-based (two instances with the same property values are equal), which is useful for caching and tests.


---