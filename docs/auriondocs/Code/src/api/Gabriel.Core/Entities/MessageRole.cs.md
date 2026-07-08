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


Represents the role of a chat message within the conversation workflow. It enables downstream components to distinguish system prompts, user inputs, assistant replies, and tool-observation messages that carry the results of tool invocations.

## Remarks

This enumeration provides a stable categorization of message origins, which simplifies routing, rendering, and audit trails across the chat pipeline. The Tool member specifically denotes an observation message that carries the outcome of a tool invocation; the related ToolCallId on the message links this observation to the corresponding assistant tool_call, preserving traceability across turns. Because the numeric values are used for serialization, be cautious when evolving the enum (adding, removing, or reordering members) to avoid breaking persisted data or cross-version compatibility.

## Example

```csharp
// Example usage: dispatching handling by message role
switch (role)
{
    case MessageRole.System:
        // system-level instructions
        break;
    case MessageRole.User:
        // user-provided input
        break;
    case MessageRole.Assistant:
        // assistant response
        break;
    case MessageRole.Tool:
        // observation derived from a tool invocation
        break;
}
```

## Notes

- If the enum is serialized as an integer, changing member order or values can break existing stored data; treat the numeric mapping as part of a stable contract.
- The Tool branch is meaningful only when you actually generate tool observations; otherwise, this value may remain unused.