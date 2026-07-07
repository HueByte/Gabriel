# MessageRole

> **File:** `src/api/Gabriel.Core/Entities/MessageRole.cs`  
> **Kind:** enum

```csharp
public enum MessageRole
{
    System = 0,
    User = 1,
    Assistant = 2,

    Tool = 3,
}
```


Represents the origin or role of a message within a chat or agent-driven workflow. Use MessageRole to distinguish between system prompts, user input, assistant responses, and observed results from tool invocations, enabling consistent rendering, routing, and policy application without relying on magic numbers.

## Remarks
This enum centralizes message provenance in the conversation flow. System messages drive prompts and policies, User messages carry human input, Assistant messages contain model-generated responses, and Tool messages carry the results of tool invocations observed by the system. The Tool value ties to a specific tool invocation context, enabling proper linkage between a tool’s invocation and its observed outcome.

## Example
```csharp
MessageRole role = MessageRole.User;
switch (role)
{
    case MessageRole.System:
        // Apply system-level formatting or policy
        break;
    case MessageRole.User:
        // Render as user input
        break;
    case MessageRole.Assistant:
        // Render as assistant response
        break;
    case MessageRole.Tool:
        // Render as tool invocation result
        break;
}
```

## Notes
- Tool messages are reserved specifically for tool invocation results; using Tool for non-tool-related observations may confuse the rendering and routing logic.
- Do not rely on the underlying numeric values; reference the named members to future-proof against potential reordering or augmentation of the enum.