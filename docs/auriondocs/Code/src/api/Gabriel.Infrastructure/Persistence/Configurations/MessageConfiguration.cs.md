# MessageConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/MessageConfiguration.cs`  
> **Kind:** class

```csharp
public class MessageConfiguration : IEntityTypeConfiguration<Message>
```


MessageConfiguration is an Entity Framework Core Fluent API configuration class that maps the Message entity to the Messages table and defines its persistence rules. Implementing `IEntityTypeConfiguration<Message>`, it centralizes how Message is stored: the table name, primary key, property constraints (including a conversion for Role), and the indexes that optimize common queries by Conversation and variant tracking. This class is the canonical place to express schema decisions that are not expressed as data annotations on the entity itself, such as complex constraints, optional fields, and performance-oriented indexes.