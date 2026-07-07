# ConversationConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/ConversationConfiguration.cs`  
> **Kind:** class

```csharp
public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
```


Configures the EF Core model for the Conversation aggregate. This class implements IEntityTypeConfiguration&lt;Conversation&gt; and directs how conversations are persisted: the table name, primary key, required fields, optional fields, and the relationships that compose the aggregate. It also defines the indices used by common dashboard queries and maps the private Messages backing field to ensure mutations go through the domain's invariants.

## Remarks
By mapping the private _messages backing field, callers mutate the Messages collection through the aggregate, preserving encapsulation and invariants. Several properties are intentionally nullable or constrained to support lazy initialization or partial data: ProjectId and StateJson are optional, while PatternOverride and PaletteOverride have length limits to guard database size. The two indices (UserId, UpdatedAt) and (ProjectId, UpdatedAt) optimize dashboard and project-filter queries; cascade delete ensures Messages are removed when a Conversation is deleted, preserving referential integrity.

## Notes
- The Messages navigation uses a private backing field and Field access mode, so EF mutates the _messages collection strictly through the aggregate.
- Mode is stored as an int (nullable) to represent the ConversationState enum; null implies the default behavior at read time.
- If new properties are added to Conversation that participate in the model, update this configuration accordingly and re-run migrations to keep the schema in sync.