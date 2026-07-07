# SendMessageRequest

> **File:** `src/api/Gabriel.API/Contracts/Messages/SendMessageRequest.cs`  
> **Kind:** record

```csharp
public record SendMessageRequest(string Content)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Content` | `string` | — |


SendMessageRequest is a simple, immutable record that represents the payload required to send a message. It carries a single property, Content, which holds the text of the message to be delivered.

## Remarks
Being a record, SendMessageRequest provides value-based equality and immutable semantics, making it easy to compare and create variants with a with-expression. It serves as a clean contract between the client and the messaging API, separating message content from transport concerns. If future API changes require more fields (such as recipient or metadata), introduce a dedicated request type rather than expanding this one.

## Example
```csharp
var request = new SendMessageRequest("Hello, world!");
// You can create a modified copy with 'with':
var next = request with { Content = "Hello again!" };
```

## Notes
- Content is provided via the constructor and is non-nullable in nullable-enabled contexts; ensure you supply a non-null value.
- This type is immutable; to produce a variation, use the with-expression to create a new instance.