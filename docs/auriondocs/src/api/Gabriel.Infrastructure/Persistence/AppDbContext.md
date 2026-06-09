# AppDbContext

> **File:** `src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs`  
> **Kind:** class

A DbContext implementation for the application's persistence layer that extends IdentityDbContext with a Guid primary key. Use this class when configuring EF Core for the application (dependency injection, migrations, and runtime data access); it exposes DbSet properties for the application's domain entities and integrates ASP.NET Identity tables.

## Remarks
AppDbContext delegates identity-related model configuration to IdentityDbContext by calling base.OnModelCreating(modelBuilder) first, then applies the application's IEntityTypeConfiguration implementations from the same assembly. It also centralizes a convention for DateTimeOffset properties (using DateTimeOffsetToBinaryConverter) to ensure correct ordering and comparison behavior on providers such as SQLite that lack a native DateTimeOffset type.

## Example
```csharp
// Registering the context in Startup/Program
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

// Consuming via constructor injection
public class ConversationService
{
    private readonly AppDbContext _db;
    public ConversationService(AppDbContext db) => _db = db;

    public async Task<List<Conversation>> GetAllAsync()
    {
        return await _db.Conversations.ToListAsync();
    }
}
```

## Notes
- The DateTimeOffset conversion is applied globally: all properties of type DateTimeOffset are persisted using DateTimeOffsetToBinaryConverter, which ensures SQL-side comparisons/sorts work on providers without native support but changes the on-disk representation.
- ApplyConfigurationsFromAssembly looks for IEntityTypeConfiguration implementations in the AppDbContext assembly; move configuration classes there or they won't be picked up automatically.
- The DbSet properties use `Set<T>`() (expression-bodied properties) and are non-nullable at runtime; for unit testing prefer an in-memory provider or an abstraction over the context if you need to mock behavior.