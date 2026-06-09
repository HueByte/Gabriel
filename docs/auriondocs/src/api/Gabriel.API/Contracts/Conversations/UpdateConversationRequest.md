# UpdateConversationRequest

> **File:** `src/api/Gabriel.API/Contracts/Conversations/UpdateConversationRequest.cs`  
> **Kind:** record

A minimal Data Transfer Object used to carry a new title when updating an existing conversation. Reach for this record as the shape of the HTTP/API request body or command payload when a client or service needs to change only the conversation's Title.

## Remarks
This positional record serves as a concise API contract for update operations that affect a conversation's title. Using a record makes the payload value-oriented (structural equality, readable deconstruction) and encourages immutable usage patterns: consumers typically receive an instance from model binding and pass it to a handler/service that applies validation and persistence.

## Example
```csharp
// Creating the request (e.g. in a client or test)
var req = new UpdateConversationRequest("New conversation title");

// Typical usage in a controller action (model-bound parameter)
[HttpPut("/conversations/{id}")]
public IActionResult Update(Guid id, UpdateConversationRequest request)
{
    // validate request.Title, call service to persist change, etc.
    conversationService.UpdateTitle(id, request.Title);
    return NoContent();
}

// With-expressions can be used to create modified copies if needed
var modified = req with { Title = "Another title" };
```

## Notes
- The record itself does not perform validation (null/empty/length); validate Title before persisting.
- Because this is a value-type record, equality is structural: two instances with the same Title are considered equal.