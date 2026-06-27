# MessageConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/MessageConfiguration.cs`  
> **Kind:** class

Configures how the Message entity is mapped to the database for Entity Framework Core: table name, primary key, property mappings (including an enum-to-int conversion for Role), nullability semantics for fields that are only populated for certain message roles, and query-focused indexes. Use this configuration when building the EF model (ApplyConfiguration) so the Message table and its indexes match the application's query patterns and role-dependent storage rules.

## Remarks
This class centralizes the persistence rules and query-oriented indexes for Message so the domain model's role-specific storage behaviour is expressed in the schema configuration rather than scattered in migrations or ad-hoc queries. It encodes several semantic conventions: Role is persisted as an integer, some fields are intentionally nullable because they are only set for particular message roles (tool calls, assistant reasoning, etc.), and the VariantGroupId/IsActiveVariant pair supports tracking alternate/generated variants of a single turn and choosing the active variant efficiently.

## Example
```csharp
// In your DbContext.OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfiguration(new MessageConfiguration());
    base.OnModelCreating(modelBuilder);
}
```

## Notes
- Several properties are nullable by design (Content, ToolCallId, ToolCallsJson, ReasoningContent); the database will not enforce role-dependent presence — that logic lives in application code.
- Role is stored with `HasConversion<int>`(), so changing the enum order or values in code will affect persisted values; preserve enum numeric stability or provide a migration strategy.
- VariantGroupId is required and is used to group regenerate siblings; for singletons this equals the Message.Id. IsActiveVariant is also required and used to mark the chosen variant within the group.
- ToolCallId has a MaxLength(64) limit; ensure any generated tool-call identifiers fit this length to avoid truncation/errors at the database layer.
- The two indexes (ConversationId + CreatedAt and ConversationId + VariantGroupId) are chosen for common filtering patterns (conversation timelines and provider-history/variant lookups) and assume queries will include ConversationId to be effective.