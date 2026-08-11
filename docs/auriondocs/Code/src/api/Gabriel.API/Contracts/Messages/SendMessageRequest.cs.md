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


SendMessageRequest is an immutable data contract used to convey the content of a message to be sent via the messaging API. As a C# record with a single positional parameter Content, it provides a concise, value-based payload that can be serialized to JSON and transmitted to the endpoint. Use this type whenever you need a strongly-typed request body for sending a message, rather than passing a raw string.

## Remarks
Because it is a single-property record, it benefits from value-based equality by Content, simple construction, and deconstruction when you need to access the content. The Content property name aligns with common API payload conventions, ensuring predictable serialization across serializers that map property names directly to JSON fields.

## Notes
- Content is non-nullable in this signature. Callers must supply a non-null string. If the API allows empty or optional content, consider using string? Content or validating before construction.
- As a record, instances are immutable; after creation, Content cannot be changed, which helps preserve message integrity across layers.