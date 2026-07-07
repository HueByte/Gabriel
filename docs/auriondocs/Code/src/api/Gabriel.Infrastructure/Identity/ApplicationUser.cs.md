# ApplicationUser

> **File:** `src/api/Gabriel.Infrastructure/Identity/ApplicationUser.cs`  
> **Kind:** class

```csharp
public class ApplicationUser : IdentityUser<Guid>
```


Represents an application user with a Guid key that carries per-user chat preferences. PreferredProvider and PreferredModel optionally specify the provider name (matching IChatProvider.Name) and the wire-level model identifier; when unset, the agent falls back to the active provider/model defined in configuration.

## Remarks
Stays in Infrastructure because identity persistence concerns belong there; Core only references users by Guid. By storing a provider name and a wire model identifier as plain strings, this class decouples domain logic from concrete providers and enables safe evolution of the provider catalog. The design relies on the global configuration to supply defaults when a user has not expressed a preference.

## Notes
- Null values trigger fallback to the configured defaults; treat absence of a preference as the system-wide default.
- If you set these values, ensure they correspond to an existing IChatProvider.Name and a valid provider model identifier to avoid mismatches at runtime.