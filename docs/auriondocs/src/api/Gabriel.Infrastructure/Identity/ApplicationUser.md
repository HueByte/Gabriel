# ApplicationUser

> **File:** `src/api/Gabriel.Infrastructure/Identity/ApplicationUser.cs`  
> **Kind:** class

Represents an application user stored via ASP.NET Identity using a Guid primary key. Use this type anywhere the system needs a persisted user record and to capture per-user preferences for chat provider and model selection (the Core project only refers to users by their Guid id, so this implementation lives in the Infrastructure layer).

## Remarks
This class extends `IdentityUser<Guid>` to keep identity persistence consistent with the rest of the domain (Guid keys). It adds two nullable preference properties — PreferredProvider and PreferredModel — which allow an agent to use a user-specific chat provider or model instead of the globally configured default. PreferredProvider is expected to match an IChatProvider.Name, and PreferredModel is the provider-specific wire-level model identifier; both are stored as plain strings and rely on the model catalog/configuration to handle stale or inactive entries.

## Example
```csharp
// Create a new user and set chat preferences
var user = new ApplicationUser
{
    Id = Guid.NewGuid(),
    UserName = "alice@example.com",
    Email = "alice@example.com",
    PreferredProvider = "grok",
    PreferredModel = "grok-4-latest"
};

// Persist with UserManager<ApplicationUser> (standard Identity workflow)
await userManager.CreateAsync(user, "P@ssw0rd!");
```

## Notes
- PreferredProvider and PreferredModel are nullable; when unset, the runtime falls back to the active provider/model defined in configuration (Providers:*:Models with IsActive=true).
- PreferredProvider must match an IChatProvider.Name exactly; misspellings or non-existent names will cause the system to use the configured default instead.
- The class only stores string identifiers and does not validate whether a model or provider is currently available — the model/catalog layer is responsible for resolving or falling back from stale references.