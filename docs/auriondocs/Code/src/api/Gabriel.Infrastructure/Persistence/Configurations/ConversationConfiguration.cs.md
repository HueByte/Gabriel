# ConversationConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/ConversationConfiguration.cs`  
> **Kind:** class

```csharp
public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
```


This class defines the EF Core mapping for the Conversation aggregate, telling the ORM how to persist Conversations: table name, keys, columns, indexes, and the one-to-many link to Messages. It also enforces encapsulation of the Messages collection via a private backing field and supports lazy-populated fields like StateJson and SummarizedThroughMessageId, plus a nullable ProjectId to ease migrations.

## Remarks
By separating persistence concerns into ConversationConfiguration, the domain model remains clean and the schema can evolve independently via migrations. The configuration centralizes common query shapes (e.g., indexes on UserId/UpdatedAt and ProjectId/UpdatedAt) and supports lazy initialization of derived state while preserving the aggregate's invariants through a controlled Messages collection.

## Notes
- Field-backed Messages: The navigation is configured to use a private backing field (_messages), enforcing encapsulation of the Messages collection.
- Cascade delete: Deleting a Conversation cascades to its Messages.
- Nullable ProjectId: ProjectId is nullable to support pre-migration conversations and backfill when a user visits a project.