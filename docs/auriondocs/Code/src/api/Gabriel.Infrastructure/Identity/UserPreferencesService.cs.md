# UserPreferencesService

> **File:** `src/api/Gabriel.Infrastructure/Identity/UserPreferencesService.cs`  
> **Kind:** class

```csharp
internal sealed class UserPreferencesService : IUserPreferences
```


UserPreferencesService is a lightweight IUserPreferences implementation that persists a small set of per-user preferences directly on ApplicationUser via UserManager, avoiding a separate table for cheap, in-place updates. GetAsync returns a UserPreferences with null values when there is no authenticated user, while SetPreferredModelAsync requires authentication and updates the user's PreferredProvider and PreferredModel (normalizing empty strings to null to represent 'unset'), throwing UnauthorizedAccessException if there is no user and InvalidOperationException for not-found users or update failures.

## Remarks
It centers per-user preferences within the identity model, drawing on ICurrentUser for the active user and `UserManager<ApplicationUser>` to read and persist fields. By routing through ApplicationUser, it relies on existing identity lifecycle, which ensures consistent validations and hooks. The normalization of empty inputs to null makes the catalog's 'unset' path behave identically regardless of client input.

## Notes
- SetPreferredModelAsync requires an authenticated user; calling it without authentication will throw UnauthorizedAccessException.
- GetAsync safely returns default preferences when there is no authenticated user, avoiding exceptions in non-UI or background contexts.