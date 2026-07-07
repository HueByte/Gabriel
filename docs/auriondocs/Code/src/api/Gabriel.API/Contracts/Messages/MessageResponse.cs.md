# MessageResponse.cs

> **Source:** `src/api/Gabriel.API/Contracts/Messages/MessageResponse.cs`

## Contents

- [MessageResponse](#messageresponse)
- [MessageToolCall](#messagetoolcall)

---

## MessageResponse
> **File:** `src/api/Gabriel.API/Contracts/Messages/MessageResponse.cs`  
> **Kind:** record

```csharp
public record MessageResponse(
    Guid Id,
    string Role,                                   
    string? Content,                               
    DateTimeOffset CreatedAt,
    Guid VariantGroupId,                           
    int VariantIndex,                              
    int VariantCount,                              
    IReadOnlyList<Guid> VariantSiblingIds,         
    string? ToolCallId = null,                     
    IReadOnlyList<MessageToolCall>? ToolCalls = null, 
    string? ReasoningContent = null                
)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Id` | `Guid` | — |
| `Role` | `string` | — |
| `Content` | `string?` | — |
| `CreatedAt` | `DateTimeOffset` | — |
| `VariantGroupId` | `Guid` | — |
| `VariantIndex` | `int` | — |
| `VariantCount` | `int` | — |
| `VariantSiblingIds` | `IReadOnlyList<Guid>` | — |
| `ToolCallId` | `string?` | `null` |
| `ToolCalls` | `IReadOnlyList<MessageToolCall>?` | `null` |
| `ReasoningContent` | `string?` | `null` |


Represents a single message response in the Gabriel API messaging model, including identity, role, content, and timing, together with metadata that tracks regenerated variants and tool-invocation details. Use this instead of plain message data when you need to handle alternate replies (VariantGroupId, VariantIndex, VariantCount, VariantSiblingIds) and any associated ToolCalls, ToolCallId, or ReasoningContent.

## Remarks
Why this exists: it centralizes both the content and the lifecycle metadata for a message, enabling clients to navigate and render regenerated variants. The VariantGroupId groups siblings that belong to the same turn, while VariantIndex and VariantCount describe the ordering and count of those siblings, and VariantSiblingIds provides the exact sequence. ToolCallId and ToolCalls tie tool invocations to the corresponding assistant message, and ReasoningContent exposes optional internal reasoning when provided by capable providers.

## Notes
- Content may be null for messages that deliver only tool results or reasoning content; rely on ReasoningContent or ToolCalls in that case.
- VariantGroupId is equal to Id for singleton (non-regenerated) turns; for regenerated turns, all siblings share the same VariantGroupId.
- ToolCalls is non-empty only for assistant messages that requested tool calls; ToolCallId is set on messages with a tool role.

---

## MessageToolCall
> **File:** `src/api/Gabriel.API/Contracts/Messages/MessageResponse.cs`  
> **Kind:** record

```csharp
public record MessageToolCall(string Id, string Name, string ArgumentsJson)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Id` | `string` | — |
| [`Name`](../../../Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) | `string` | — |
| `ArgumentsJson` | `string` | — |


MessageToolCall is an immutable record that packages the essential information to invoke a messaging tool: the tool's identifier (Id), the tool's name (Name), and a JSON-encoded set of arguments (ArgumentsJson). Developers create this object when they need to dispatch a tool invocation through a dispatcher or runner rather than calling the tool directly, enabling consistent transport and logging of tool invocations.

## Remarks
MessageToolCall acts as a lightweight value object that represents a tool invocation boundary. By storing ArgumentsJson as a string, the system can evolve the argument schema without changing the type structure, while Id/Name identify the target and operation for the tool runner.

## Example
```csharp
var call = new MessageToolCall("tool-123", "SendMessage", "{\"recipient\":\"user42\",\"text\":\"Hello\"}");
```

## Notes
- ArgumentsJson must be valid JSON; otherwise the tool may fail to execute.
- Id should uniquely identify the targeted tool invocation to avoid routing confusion.
- Be careful with sensitive data in ArgumentsJson; avoid leaking it through logs or telemetry.

---