# ConversationConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/ConversationConfiguration.cs`  
> **Kind:** class

Configures the EF Core mapping for the Conversation entity: table name, primary key, property constraints (requiredness and max lengths), indexes used by common queries, nullable rolling-summary and state columns, enum storage for per-conversation mode, and the relationship to Message entities.

## Remarks
This configuration centralises persistence rules for the Conversation aggregate so the domain model can enforce invariants while the database schema supports the application's hot queries (sidebar and project filters) and lifecycle (cascade delete of messages). It intentionally maps the Messages navigation to a private backing field (_messages) and sets PropertyAccessMode.Field so callers must mutate messages via the aggregate API rather than manipulating the collection directly; this preserves aggregate encapsulation at the ORM level.

## Example
```csharp
// In your DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfiguration(new ConversationConfiguration());
}
```

## Notes
- The source comments mention an "UpdatedAt desc" index for the sidebar, but the HasIndex call does not specify ordering; if you need a descending index in the database, add explicit ordering in the migration or index definition.
- ProjectId, Summary, SummarizedThroughMessageId, and StateJson are left nullable to allow in-place migrations and lazy backfill; code reading these fields should handle null as "not yet populated." 
- Mode is stored as the enum's numeric value (int) and is nullable; null is treated as the default behavior (Chatty) at read time.
- Messages are configured with cascade delete: removing a Conversation will delete its Messages in the database.
- Title and Pattern/Palette overrides have explicit max lengths (256 and 32) — changing these requires a schema migration.