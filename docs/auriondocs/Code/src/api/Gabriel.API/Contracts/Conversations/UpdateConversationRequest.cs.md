# UpdateConversationRequest

> **File:** `src/api/Gabriel.API/Contracts/Conversations/UpdateConversationRequest.cs`  
> **Kind:** record

Represents the payload for an API call that updates a conversation's title. Use this record as the request DTO when an endpoint accepts a new Title for an existing conversation (e.g., via an HTTP PUT/PATCH body).

## Remarks
This is a lightweight positional record used as an API contract/DTO. Being a record, it provides value-based equality, a concise deconstruct pattern, and (in modern C#) init-only semantics for its generated property. The type intentionally contains only the data needed to perform the update — validation (length, emptiness, allowed characters, etc.) and authorization should be handled by the caller or by pipeline components (model validators, filters) rather than by the DTO itself.

## Example
```csharp
// JSON body sent by a client
// { "title": "New conversation title" }

// Typical controller action binding the request body
[HttpPut("{id}")]
public IActionResult UpdateConversation(Guid id, UpdateConversationRequest request)
{
    // request.Title contains the new title
    // validate and apply the update through application services
    return NoContent();
}

// Creating an instance in code
var req = new UpdateConversationRequest("A better title");
var (title) = req; // deconstructs the record
```

## Notes
- The record contains no built-in validation; callers must validate Title (null/empty checks, length limits, sanitization) before applying changes.
- As a positional record the property is generated with init semantics and the type uses value equality — two instances with the same Title are considered equal.
- If the project does not enable C# nullable reference types, Title may be null at runtime even though the signature declares a non-nullable string.