# UserPreferencesService

> **File:** `src/api/Gabriel.Infrastructure/Identity/UserPreferencesService.cs`  
> **Kind:** class

```csharp
internal sealed class UserPreferencesService : IUserPreferences
```


Implements IUserPreferences by reading and writing a tiny set of fields directly on ApplicationUser. GetAsync returns the current user’s preferred provider and model (or defaults if there is no authenticated user), and SetPreferredModelAsync updates those fields for the authenticated user, normalizing empty inputs to null so the catalog can treat the preference as unset.

## Remarks
This thin wrapper stores the small set of user preferences directly on the ApplicationUser to avoid a separate table; it keeps a simple, low-cost representation while avoiding friction for authenticated edits. It normalizes empty inputs to null so the catalog can consistently treat a preference as unset, and it degrades gracefully in non-authenticated contexts by returning defaults for reads while requiring authentication for updates.

## Notes
- GetAsync returns a UserPreferences instance with nulls when there is no authenticated user or the user cannot be found, enabling callers to observe a neutral default.
- SetPreferredModelAsync requires an authenticated user; otherwise it throws UnauthorizedAccessException.
- When updating, if the UserManager.UpdateAsync call fails, the exception includes all error descriptions joined by '; ' for easier diagnostics.
- Empty or whitespace provider/model values are normalized to null to signal an explicit 'unset' to downstream consumers.