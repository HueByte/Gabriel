# AppDbContext

> **File:** `src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs`  
> **Kind:** class

```csharp
public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
```


AppDbContext is the EF Core DbContext that coordinates the Gabriel.Infrastructure persistence, blending ASP.NET Core Identity with the domain data using Guid keys. It exposes DbSets for Conversations, Messages, RefreshTokens, Projects, ProjectFiles, MemoryEntries, and MetricEntries, applies configurations from the assembly, and stores DateTimeOffset properties as binary to support SQLite and comparable databases.

## Remarks
Acts as the central persistence boundary for identity and domain aggregates, shielding callers from EF Core details. By loading configurations from the assembly, it keeps mappings centralized and migrations coherent, while the DateTimeOffset conversion demonstrates provider-agnostic temporal data handling.

## Notes
- Always call base.OnModelCreating when deriving from IdentityDbContext to ensure the AspNet* identity tables are wired up.
- DateTimeOffsetToBinaryConverter stores DateTimeOffset as a long; if you later switch to a database provider with native DateTimeOffset support, you may be able to remove this converter.