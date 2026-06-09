# ConversationConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/ConversationConfiguration.cs`  
> **Kind:** class

Configures how the Conversation aggregate is mapped to the database schema for Entity Framework Core. Use this when building the EF model (for example in DbContext.OnModelCreating) to ensure table name, keys, property constraints, indexes, navigation mapping and cascade behavior match the application's storage and query expectations.

## Remarks
This class centralizes schema decisions for the Conversation entity: it sets the table name and primary key, declares required and length-constrained properties, defines indexes optimized for the dashboard and project-scoped queries, and configures several nullable columns that are populated lazily (such as project membership, rolling summary fields, and serialized conversation state). It also maps the Messages navigation to a private backing field and enforces cascade delete so messages are removed when their parent conversation is deleted.

## Example
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfiguration(new ConversationConfiguration());
}
```

## Notes
- The ProjectId, Summary and StateJson columns are intentionally nullable to support migrations and lazy backfill; calling code should handle their absence until populated.
- The Mode property is stored as the enum's integer value; adding new enum members is a Core enum definition change and will not require altering storage format.
- The Messages navigation is mapped to a private backing field named `_messages`; renaming or removing that field will break the mapping and can cause runtime errors.