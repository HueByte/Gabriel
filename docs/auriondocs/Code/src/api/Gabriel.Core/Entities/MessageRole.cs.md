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


Defines the origin of a chat message in the Gabriel.Core messaging model. Each MessageRole indicates who produced the message in a multi-turn conversation: System messages establish context, User messages originate from the end user, Assistant messages are model responses, and Tool messages carry the results of an external tool invocation. The Tool variant includes a ToolCallId that links the observation back to the corresponding tool invocation, enabling traceability and correct routing of tool outputs within the dialog.

## Remarks
This abstraction separates message semantics from content, allowing the UI, logging, prompt construction, and routing logic to treat each role differently. In tool-augmented workflows, Tool messages turn tool results into first-class observations, preserving how a user goal was achieved and enabling distinct presentation and auditing of tool outputs within the conversation.

## Example
```csharp
// Example usage illustrating roles in a conversation
var conv = new List<Message>
{
    new Message { Role = MessageRole.System, Content = "You are a helpful assistant." },
    new Message { Role = MessageRole.User, Content = "Translate this to French." },
    new Message { Role = MessageRole.Assistant, Content = "Voici la traduction." },
    // After invoking an external tool, the result is represented as a Tool message
    new Message { Role = MessageRole.Tool, Content = "Tool result: translated text", ToolCallId = "tool-456" }
};
```

## Notes
- ToolCallId must reference the corresponding tool invocation; without it, the tool observation cannot be linked to its source.
- Tool messages represent observed tool outputs and should not be treated as ordinary assistant text; maintain a clear distinction in history and UI.
- When persisting or transmitting messages, ensure the numeric Role values remain in sync with the enum to avoid misinterpretation during deserialization.