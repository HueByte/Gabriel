# SetConversationModeRequest

> **File:** `src/api/Gabriel.API/Contracts/Conversations/SetConversationModeRequest.cs`  
> **Kind:** record

A small DTO used as the request body for updating a conversation's mode. Send the lowercased mode name (one of: "chatty", "elaborative", "concise", "tutor", "critic") to select that mode, or null to clear the override and return the conversation to the default mode (the API treats a cleared/absent mode as "chatty" when read back).

## Remarks
This record models the minimal payload accepted by the PUT /api/conversations/{id}/mode endpoint: a single nullable string representing the desired conversation mode. It exists to keep the HTTP contract simple and language-agnostic (the client only needs to supply a lowercased enum name or null), rather than shipping the full enum type from the client library.

## Example
```csharp
// Create a request to set the conversation to "tutor"
var req = new SetConversationModeRequest("tutor");
// JSON body produced by typical serializers: { "Mode": "tutor" }

// To clear the override and revert to default:
var clearReq = new SetConversationModeRequest(null);
// JSON body: { "Mode": null }
```

## Notes
- The server expects the enum name in lowercase; use the exact lowercase names (chatty, elaborative, concise, tutor, critic).
- Use null (not an empty string) to clear the conversation mode so the server will treat it as the default (read-time default is treated as "chatty").
- The record contains no client-side validation; callers should ensure the value is one of the supported names to avoid server-side rejection.