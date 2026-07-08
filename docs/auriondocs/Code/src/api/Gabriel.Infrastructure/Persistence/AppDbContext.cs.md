# AppDbContext

> **File:** `src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs`  
> **Kind:** class

```csharp
public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
```


AppDbContext is the Entity Framework Core DbContext that serves as the data access hub for Gabriel's persistence layer. It inherits from IdentityDbContext<ApplicationUser, `IdentityRole<Guid>`, Guid>, thereby wiring in ASP.NET Identity tables alongside the application’s domain entities. It exposes DbSet properties for Conversations, Messages, RefreshTokens, Projects, ProjectFiles, MemoryEntries, and MetricEntries, which EF Core uses to map and query these aggregates. In OnModelCreating, it first delegates identity-related configuration to the base implementation, then applies all entity-type configurations discovered in this assembly via ApplyConfigurationsFromAssembly, ensuring consistent mappings across the model. It also overrides ConfigureConventions to persist DateTimeOffset properties using a DateTimeOffsetToBinaryConverter, addressing SQLite's lack of native DateTimeOffset support and ensuring correct ordering and comparisons in SQL. This context is intended to be registered in the DI container and used by repositories and services to read and write application data.