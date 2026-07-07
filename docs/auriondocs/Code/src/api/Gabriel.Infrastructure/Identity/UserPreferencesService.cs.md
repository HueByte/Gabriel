# UserPreferencesService

> **File:** `src/api/Gabriel.Infrastructure/Identity/UserPreferencesService.cs`  
> **Kind:** class

```csharp
internal sealed class UserPreferencesService : IUserPreferences
```


This internal sealed class exposes and persists a small set of user preferences directly on ApplicationUser, avoiding an extra table by using the Identity store as its backing. It implements IUserPreferences and provides GetAsync for reading the current user’s PreferredProvider and PreferredModel and SetPreferredModelAsync for updating them, with authentication required for writes and graceful handling when there is no authenticated user during reads.

## Remarks
By colocating preferences on ApplicationUser, the service minimizes complexity and leverages existing persistence and validation paths. Empty or whitespace strings are normalized to null to ensure the catalog's unset path is consistent regardless of input. Reads are resilient in contexts without an authenticated user (e.g., background tasks) and simply yield unset values instead of throwing; writes are guarded by authentication and will fail fast if the user cannot be located or the update fails. All persistence goes through UserManager, which ties the behavior to the Identity stack's lifecycle and error reporting.

## Notes
- GetAsync returns UserPreferences with null values when no authenticated user or user cannot be found; callers must interpret this as "unset".
- SetPreferredModelAsync requires an authenticated user; attempting to call while unauthenticated throws UnauthorizedAccessException.
- The cancellation token ct is accepted but not propagated to FindByIdAsync/UpdateAsync in the current implementation (potential cancellation improvement).