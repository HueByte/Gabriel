# SendMessageRequest

> **File:** `src/api/Gabriel.API/Contracts/Messages/SendMessageRequest.cs`  
> **Kind:** record

Represents the payload for sending a message: a simple, immutable contract that carries the message text to be delivered by an API endpoint or messaging service. Use this record when you need a typed request object that contains only the message content.

## Remarks
This is a positional record with a single property, Content, making it a small DTO/contract type. As a record it provides value-based equality, deconstruction, and support for `with` expressions; these behaviors make it convenient for tests and scenarios where immutability and structural comparisons are desirable.

## Example
```csharp
// Create a new request and pass it to a handler or controller
var request = new SendMessageRequest("Hello, world!");
await messageService.SendAsync(request);

// Make a copy with a modified content
var updated = request with { Content = "Updated message" };
```

## Notes
- The code does not declare nullable annotations here; if your project enables nullable reference types, consider validating or annotating Content as needed to express whether null is allowed.
- Equality for instances of this record is based on the Content value (value equality), not reference identity.