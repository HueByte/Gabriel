# MessageConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/MessageConfiguration.cs`  
> **Kind:** class

```csharp
public class MessageConfiguration : IEntityTypeConfiguration<Message>
```


MessageConfiguration defines how the Message entity is mapped to the database: it binds the entity to the Messages table, declares Id as the primary key, enforces required fields like ConversationId and CreatedAt, stores Role as an int via an enum conversion, and configures property constraints and indexes to support efficient querying and variant history.

## Remarks
This configuration centralizes the database schema for Message and encodes domain concepts used by the application, such as VariantGroupId and IsActiveVariant, which group regen siblings and enable provider-history filtering. The included indexes optimize common access patterns: the composite index on ConversationId and CreatedAt enables efficient chronological retrieval of messages within a conversation, while the index on ConversationId and VariantGroupId supports rapid filtering when exploring or activating a specific variant.

## Notes
- Be aware that several properties are nullable in the domain model (Content, ToolCallsJson, ReasoningContent); callers should handle nullability when composing reads or writes.
- Role is stored as int due to `HasConversion<int>`(), so adding new enum values requires coordination with the database values and migrations.