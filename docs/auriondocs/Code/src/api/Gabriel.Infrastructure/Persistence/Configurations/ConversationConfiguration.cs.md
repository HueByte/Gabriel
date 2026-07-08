# ConversationConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/ConversationConfiguration.cs`  
> **Kind:** class

```csharp
public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
```


Configures how EF Core persists the Conversation aggregate. It maps to the Conversations table, defines the primary key and property constraints, sets up indices for the hot dashboard queries, and wires the one-to-many relationship to Message entities. It also maps the Conversations.Messages collection to a private backing field to ensure mutations flow through the aggregate and supports lazy or backfilled fields such as ProjectId and StateJson.

## Remarks
This configuration centralizes persistence concerns in one place, keeping EF Core details out of the domain entity. It encodes practical decisions for schema shape, query performance, and aggregate boundaries—such as nullable ProjectId to survive migrations with lazy backfill, and a field-backed Messages collection to enforce mutation through the aggregate. By configuring the state (StateJson) and optional attributes (PatternOverride, PaletteOverride) thoughtfully, it supports forward-compatibility and lazy initialization patterns without disturbing the in-domain logic.

## Notes
- Nullable ProjectId is intentional to preserve existing conversations during migrations; backfilled lazily when the user first creates or visits a project.
- The rolling-summary columns (Summary and SummarizedThroughMessageId) are nullable until the conversation crosses the compact threshold, so consumers must handle absence of these values.
- The Messages navigation uses a private backing field (_messages) with Field access mode to ensure all mutations go through the aggregate, preserving invariants and encapsulation.