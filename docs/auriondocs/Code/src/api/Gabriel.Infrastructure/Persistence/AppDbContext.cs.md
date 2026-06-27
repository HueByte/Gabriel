# AppDbContext

> **File:** `src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs`  
> **Kind:** class

Represents the Entity Framework Core DbContext for the application and the identity store. Use this class as the central persistence entry point (register it with DI and use it for queries/commands or repositories) when your application uses ApplicationUser with Guid keys and the project's domain entities (Conversation, Message, RefreshToken, Project, ProjectFile, MemoryEntry, MetricEntry).

## Remarks
This class inherits from IdentityDbContext<ApplicationUser, `IdentityRole<Guid>`, Guid> so the ASP.NET Identity schema and behavior are wired up automatically. OnModelCreating calls the base implementation first (to ensure the Identity model is configured) and then applies any `IEntityTypeConfiguration<T>` implementations found in the same assembly via ApplyConfigurationsFromAssembly. ConfigureConventions installs a conversion for DateTimeOffset values (DateTimeOffsetToBinaryConverter) to persist them as a long — this addresses SQLite's lack of a native DateTimeOffset type and enables SQL-side ordering and comparisons.

## Example
```csharp
// In Startup.cs / Program.cs (ConfigureServices)
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("Default")));

// Identity must be configured to use the same key types (Guid)
services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
```

## Notes
- Ensure ApplicationUser and any Identity usage in the app use Guid as the primary key type to match the IdentityDbContext generic parameters.
- The DateTimeOffset conversion stores values as a long in the database; the column will not be a datetime type and direct SQL queries against the raw value must account for that encoding.
- Entity configurations must be implemented as `IEntityTypeConfiguration<T>` in this assembly (or otherwise discovered) for ApplyConfigurationsFromAssembly to pick them up.