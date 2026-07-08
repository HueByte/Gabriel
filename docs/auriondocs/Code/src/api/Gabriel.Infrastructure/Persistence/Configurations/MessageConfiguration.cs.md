# MessageConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/MessageConfiguration.cs`  
> **Kind:** class

```csharp
public class MessageConfiguration : IEntityTypeConfiguration<Message>
```


MessageConfiguration configures how EF Core maps the Message entity to the database schema. It centralizes all Fluent API rules for Message, including the table name, primary key, required fields, a role-to-integer conversion, optional fields for different message roles, and the indexes that support efficient per-conversation retrieval and variant-history lookups.

## Remarks
This abstraction encapsulates persistence concerns for Message, allowing the domain model to remain clean while all EF Core specifics live here and are applied during model creation. The defined indexes reflect common query patterns: (ConversationId, CreatedAt) enables fast retrieval of messages in a conversation by time, while (ConversationId, VariantGroupId) supports provider-history filtering across multiple variants within the same turn. The VariantGroupId and IsActiveVariant properties enable multi-variant turns, where the same conversational turn can exist in several variants; in singleton scenarios VariantGroupId equals the Id.

## Dependencies
- IEntityTypeConfiguration
- Message

## Dependency APIs
- class [`Message`](../../../Gabriel.Core/Entities/Message.cs.md) (`src/api/Gabriel.Core/Entities/Message.cs`)
  - property `Guid Id`
  - property `Guid ConversationId`
  - property `MessageRole Role`
  - property `string? Content`
  - property `string? ToolCallId`
  - property `string? ToolCallsJson`
  - property `string? ReasoningContent`
  - property `DateTimeOffset CreatedAt`
  - property `Guid VariantGroupId`
  - property `bool IsActiveVariant`
  - `Message()`
  - `Message Create(Guid conversationId, MessageRole role, string? content, string? toolCallId, string? toolCallsJson, Guid? variantGroupId)`
  - …and 3 more member(s) not shown

## Symbol To Document
- Name: `MessageConfiguration`
- Kind: class
- File: `src/api/Gabriel.Infrastructure/Persistence/Configurations/MessageConfiguration.cs`
- Language: csharp
- ID: 74f4e0f8-4ecb-430b-b649-4de743cd3fb9