# MessageRole

> **File:** `src/api/Gabriel.Core/Entities/MessageRole.cs`  
> **Kind:** enum

Identifies the origin/role of a chat message in the system: System, User, Assistant, or Tool. Use this enum when recording, routing, displaying or processing messages so callers and handlers can distinguish who produced a message and apply role-specific logic (e.g., rendering, permissioning, or correlating tool responses).

## Remarks
The Tool value represents an observation produced by a tool invocation rather than human or assistant text. Tool messages are expected to carry a reference (ToolCallId) that links the observation back to the assistant's tool call being answered. Numeric values are assigned explicitly and should be treated as stable for any persisted data or protocol surface.

## Example
```csharp
// Basic routing based on role
void HandleMessage(Message msg)
{
    switch (msg.Role)
    {
        case MessageRole.System:
            // internal/system instructions
            break;
        case MessageRole.User:
            // render user message in UI
            break;
        case MessageRole.Assistant:
            // render assistant reply
            break;
        case MessageRole.Tool:
            // correlate with the originating tool call using msg.ToolCallId
            break;
    }
}
```

## Notes
- Tool messages should include the Message.ToolCallId (or equivalent) so consumers can correlate the observation with the assistant's tool invocation.
- The enum values are explicit (System = 0, User = 1, Assistant = 2, Tool = 3); avoid renumbering or reordering if values are persisted or exchanged across services.
- The default enum value is System (0); initialize message.Role explicitly to avoid accidental misclassification.