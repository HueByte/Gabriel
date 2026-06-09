# UserPreferencesService

> **File:** `src/api/Gabriel.Infrastructure/Identity/UserPreferencesService.cs`  
> **Kind:** class

Reads and writes the small set of per-user preferences stored directly on ApplicationUser. Reach for this service when you need to get or persist the current authenticated user's preferred provider and model; GetAsync is safe to call when no user is authenticated (it returns an empty preference object), while SetPreferredModelAsync requires an authenticated user and persists changes via ASP.NET Identity's UserManager.

## Remarks
This implementation intentionally keeps preference fields on the ApplicationUser entity rather than introducing a dedicated table: the preferences are few and inexpensive to store, so avoiding extra schema and joins reduces ceremony. The service coordinates between ICurrentUser (to find the current subject) and UserManager (to read/update the user record). When there is no authenticated user the service behaves as a no-op for reads (returning null preferences) — callers that need a fallback should rely on catalog/config defaults.

## Example
```csharp
// Typical usage inside an API handler or controller
var prefs = await userPreferencesService.GetAsync();
if (prefs.PreferredProvider is null)
{
    // fall back to system default
}

// Update preferences for the current user
try
{
    await userPreferencesService.SetPreferredModelAsync("openai", "gpt-4");
}
catch (UnauthorizedAccessException)
{
    // no authenticated user — prompt for auth
}
catch (InvalidOperationException ex)
{
    // failed to persist changes; inspect ex.Message for details
}
```

## Notes
- The CancellationToken parameter is accepted by the public methods but is not propagated into the underlying UserManager calls; callers should not assume the underlying identity operations are cancelable.
- Empty or whitespace provider/model values are normalized to null before persisting so that "preference unset" is represented consistently.
- SetPreferredModelAsync throws UnauthorizedAccessException when there is no authenticated user and throws InvalidOperationException if the user cannot be found or if the UserManager update fails (the exception message aggregates identity error descriptions).