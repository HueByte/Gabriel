# MessageRole

> **File:** `src/api/Gabriel.Core/Entities/MessageRole.cs`  
> **Kind:** enum

Represents the origin or intent of a conversation message. Use this enum to tag messages so callers and processors can distinguish system instructions, user input, assistant responses, and tool-generated observations.

## Remarks
This enum is the canonical classification for messages exchanged within the dialog/assistant system. The values are intentionally explicit so messages can be serialized, logged, and routed (for example, forwarding tool observations to a different handler). The Tool member is special: it denotes an observation produced by executing a tool in response to an assistant's tool_call; the corresponding Message object will reference the assistant's tool_call via a ToolCallId.

## Example
```csharp
// Example: basic handling of different message roles
void HandleMessage(Message message)
{
    switch (message.Role)
    {
        case MessageRole.System:
            ApplySystemInstructions(message.Content);
            break;
        case MessageRole.User:
            EnqueueForAssistantProcessing(message);
            break;
        case MessageRole.Assistant:
            SendAssistantReplyToClient(message);
            break;
        case MessageRole.Tool:
            // Tool messages are observations answering a previous tool_call
            ProcessToolObservation(message.ToolCallId, message.Content);
            break;
    }
}
```

## Notes
- The enum values are explicit and used in serialized forms — do not renumber or remove members if messages are persisted or exchanged across services.
- Tool messages are intended to carry results of tool invocations; consumers should look for a ToolCallId on the Message to correlate the observation with the originating tool_call.
- Treat these roles as orthogonal to content: a message labeled Assistant may still include metadata that requires special handling (e.g., suggested actions).