# ApplicationUser

> **File:** `src/api/Gabriel.Infrastructure/Identity/ApplicationUser.cs`  
> **Kind:** class

```csharp
public class ApplicationUser : IdentityUser<Guid>
```


Represents an application user stored with a Guid key and wired into the infrastructure's Identity persistence. By deriving from `IdentityUser<Guid>`, it keeps identity concerns in Infrastructure while the Core projects interact with users via their Guid. It adds per-user model preferences via PreferredProvider and PreferredModel to remember a user's chosen chat provider and model; both properties are nullable so the agent can fall back to configuration defaults when unset. When unset, the agent falls back to whichever Providers:*:Models entry has IsActive=true in config.

## Remarks
This type exists in Infrastructure because Identity is a persistence concern; the Core only uses user identifiers (Guid). It centralizes per-user chat preferences: PreferredProvider maps to IChatProvider.Name and PreferredModel maps to the provider's wire-level model identifier, stored as plain strings to remain decoupled from concrete providers. The model catalog handles stale references gracefully by falling back to the default model when needed, guided by the configuration's active entries.

## Notes
- Nullable properties mean unset values trigger the global defaults for provider and model.
- Stored as plain strings can require vigilance to keep them in sync with the model catalog and available providers; rely on the catalog to resolve or fall back as configured.
- This class encapsulates persistence-related state related to identity, separating concerns from the Core domain logic that operates on user identities.