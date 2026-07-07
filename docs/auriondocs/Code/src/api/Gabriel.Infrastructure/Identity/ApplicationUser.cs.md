# ApplicationUser

> **File:** `src/api/Gabriel.Infrastructure/Identity/ApplicationUser.cs`  
> **Kind:** class

```csharp
public class ApplicationUser : IdentityUser<Guid>
```


ApplicationUser represents a persistence-backed user identity with a Guid key, extending `IdentityUser<Guid>` so the domain can reference users by ID consistently. It adds per-user model selection via nullable PreferredProvider and PreferredModel, enabling runtime customization while Core remains agnostic to specific providers or models.

## Remarks
ApplicationUser serves as a thin persistence contract for user preferences. By storing the selected provider and model as plain strings, the system can reference a central catalog and gracefully fall back to the default active model when a user has not chosen one. This design keeps identity concerns in Infrastructure and prevents Core from needing knowledge of provider specifics, while still allowing per-user customization.

## Notes
- When PreferredProvider or PreferredModel is null, the system falls back to the default active provider/model as determined by configuration (Providers:*:Models with IsActive = true).
- Because these properties are plain strings, validate against the model catalog to avoid stale references; changes to provider names or model identifiers should align with catalog data.
- There is no strict foreign-key constraint here by design, which keeps identity/persistence decoupled from provider-specific details.
