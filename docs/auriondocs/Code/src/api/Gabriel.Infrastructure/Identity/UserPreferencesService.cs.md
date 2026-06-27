# UserPreferencesService

> **File:** `src/api/Gabriel.Infrastructure/Identity/UserPreferencesService.cs`  
> **Kind:** class

A small service that implements IUserPreferences by reading and writing two preference fields (PreferredProvider and PreferredModel) directly on the ApplicationUser record via UserManager. Use this when you want the current authenticated user's lightweight preference values; calls are effectively no-ops when there is no authenticated user (GetAsync returns an empty UserPreferences), while updates require an authenticated user and will throw on failure.

## Remarks
This class keeps preferences colocated on the ApplicationUser entity rather than introducing a separate table because the stored data is minimal. It delegates identity persistence to ASP.NET Identity's UserManager and relies on ICurrentUser to determine the active user. The service normalizes empty or whitespace strings to null so callers that clear preferences produce a consistent “unset” state that downstream code (for example, catalog fallback logic) can detect.

## Example
```csharp
// Read current preferences
var prefs = await userPreferencesService.GetAsync();
Console.WriteLine($"Provider: {prefs.PreferredProvider}, Model: {prefs.PreferredModel}");

// Update preferences (throws if there is no authenticated user)
await userPreferencesService.SetPreferredModelAsync("openai", "gpt-4");

// Clear a preference: pass null or empty/whitespace; the service stores null
await userPreferencesService.SetPreferredModelAsync(null, "");
```

## Notes
- SetPreferredModelAsync throws UnauthorizedAccessException when there is no authenticated user, and InvalidOperationException if the user record cannot be found or the UserManager update fails (error details are included in the exception message).
- Empty or whitespace provider/model values are normalized to null before persisting; callers should expect a null to represent an unset preference.
- The CancellationToken parameter is accepted by the public methods but is not forwarded to UserManager methods in this implementation; cancellation therefore does not currently abort the underlying identity calls.