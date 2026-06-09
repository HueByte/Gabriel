# MessageConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/MessageConfiguration.cs`  
> **Kind:** class

Configures the Entity Framework Core mapping for the Message entity: table name, key, property mappings (including required/nullable settings), a conversion for the Role enum, max length for ToolCallId, and two composite indexes used for query filtering and ordering. Use this configuration whenever the Message entity is added to a DbContext so EF Core generates the correct schema and indexes.

## Remarks
This class centralizes persistence rules for Message instances. It enforces required fields (ConversationId, Role, CreatedAt, VariantGroupId, IsActiveVariant) and encodes several domain-level decisions into the schema: Role is stored as an integer, ToolCallId is constrained to 64 characters, and several properties are intentionally nullable because they are only populated for specific message types (tool calls, assistant tool-call aggregates, or provider "thinking" streams). Two composite indexes support common queries — one for ordering/messages in a conversation (ConversationId + CreatedAt) and one for efficiently filtering historical variants within a conversation (ConversationId + VariantGroupId). The VariantGroupId/IsActiveVariant pair implements variant grouping semantics: sibling messages produced during regeneration share a VariantGroupId (singletons use their Id as the group id), and IsActiveVariant marks which sibling is currently active.

## Notes
- Role is persisted with `HasConversion<int>`(); changing the enum's order or numeric values will affect stored data and requires care during migrations.
- ToolCallId is limited to 64 characters; callers must ensure identifiers fit this length to avoid truncation or migration errors.
- Several properties are nullable by design (Content, ToolCallId, ToolCallsJson, ReasoningContent); queries or application logic should handle absent values appropriately.
- The composite indexes are part of the schema — altering them will change query performance and may require a migration for existing databases.