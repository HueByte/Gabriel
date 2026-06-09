# SetConversationModeRequest

> **File:** `src/api/Gabriel.API/Contracts/Conversations/SetConversationModeRequest.cs`  
> **Kind:** record

A request contract for updating a conversation's mode via the PUT /api/conversations/{id}/mode endpoint. The Mode property should be the lowercased enum name (chatty, elaborative, concise, tutor, critic) or null to clear the mode back to the service default.

## Remarks
This is a simple DTO used by the API surface to carry the desired mode for a conversation. The service treats a null value as a request to clear any explicit mode and fall back to the default; at read time the default is treated as "chatty". The string value is expected to be the lowercased enum name rather than a numeric value.

## Example
```csharp
// C# client-side usage
var req = new SetConversationModeRequest("concise");
// JSON body sent to the endpoint:
// { "mode": "concise" }

// To clear the explicit mode and revert to default:
var clearReq = new SetConversationModeRequest(null);
// JSON body:
// { "mode": null }
```

## Notes
- The server expects the enum name in lowercase; mismatched casing may be rejected or ignored.
- Passing null clears the stored mode; the service will treat the absence of an explicit mode as the default (displayed as "chatty").