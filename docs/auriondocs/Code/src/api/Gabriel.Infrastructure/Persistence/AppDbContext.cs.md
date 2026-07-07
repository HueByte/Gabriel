# AppDbContext

> **File:** `src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs`  
> **Kind:** class

```csharp
public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
```


AppDbContext acts as the EF Core DbContext for the Gabriel.Infrastructure layer, combining Identity's user/role schema with the domain entities used by the application. It exposes DbSets for Conversations, Messages, RefreshTokens, Projects, ProjectFiles, MemoryEntries, and MetricEntries, making it the central point for querying and persisting application data.

## Remarks
- Centralizes data access and coordination between identity and domain models.
- It calls base.OnModelCreating(modelBuilder) before applying the project-specific configurations to ensure Identity tables are established, then custom mappings are layered on top.
- It overrides ConfigureConventions to persist DateTimeOffset as binary in SQLite via DateTimeOffsetToBinaryConverter, enabling proper ordering and comparisons in SQLite where native DateTimeOffset support is absent.

## Notes
- SQLite-specific: DateTimeOffset values are stored as 64-bit binary; this can affect migrations or data portability if you switch database providers.
- The order of configuration matters: base.OnModelCreating should be invoked before applying configurations to avoid interfering with Identity mappings.
