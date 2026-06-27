# ApplicationUser

> **File:** `src/api/Gabriel.Infrastructure/Identity/ApplicationUser.cs`  
> **Kind:** class

Represents an application user record persisted by ASP.NET Core Identity using a Guid primary key. In addition to the standard Identity fields, this type stores per-user preferences for a chat provider and a provider-specific model identifier; set these when a user should override the global provider/model defaults for their agent interactions.

## Remarks
Placed in the Infrastructure layer because Identity is a persistence concern while the Core domain only references users by their Guid id. PreferredProvider maps to an IChatProvider.Name (for example "grok") and PreferredModel is the provider's wire-level model identifier (for example "grok-4-latest"). Both properties are nullable: when unset the system falls back to the active default model from configuration. Values are persisted as plain strings — the model catalog/service is responsible for handling stale or unknown identifiers and performing any fallback behavior.

## Example
```csharp
// Create a user and set per-user model preferences. Persist using your Identity UserManager/DbContext.
var user = new ApplicationUser
{
    Id = Guid.NewGuid(),
    UserName = "alice@example.com",
    PreferredProvider = "grok",
    PreferredModel = "grok-4-latest"
};

// e.g. await userManager.CreateAsync(user, "Secret123!");
```

## Notes
- Both PreferredProvider and PreferredModel are nullable — code that selects a provider/model should treat null as "use configured default."  
- These properties are stored as plain strings and are not validated against available providers/models here; lookups and fallbacks happen at provider/catalog runtime.  
- The class inherits `IdentityUser<Guid>`, so the primary key is a Guid to align with the domain's identifier strategy.