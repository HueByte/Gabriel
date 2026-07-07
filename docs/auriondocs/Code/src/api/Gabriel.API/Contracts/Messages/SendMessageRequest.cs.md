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


SendMessageRequest is an immutable data carrier defined as a C# 9 record. It represents the payload for sending a message, carrying a single string property, Content. As a record, it provides value-based equality and straightforward serialization/deserialization across API boundaries, making it a simple, predictable DTO for messaging endpoints.

## Remarks
Use this abstraction when you want a clean, transport-friendly contract to carry message content across layers or over HTTP. The positional record captures Content as its sole data point, enabling concise deconstruction and easy pattern matching. Because it is a value-based data carrier, two requests with the same Content compare as equal.

## Notes
- This type is a pure data transfer object with no behavior; validation should occur at the API boundary if required.
- As a positional record, Content is set at construction time; to derive a modified payload, use the with-expression (e.g., var next = existing with { Content = "new" };).