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


MessageResponse represents a single chat message along with variant-generation and tool-interaction metadata. Use it when you need to store or propagate a complete turn in a regenerable conversation, including all sibling variants, any attached tool calls, and optional reasoning content, rather than just the raw text.

## Remarks
This record groups related variants via VariantGroupId and VariantSiblingIds, and it tracks the position and total count of a variant among its siblings. The VariantIndex and VariantCount enable consumers to present or iterate through regenerated turns in a deterministic order. ToolCallId and ToolCalls capture tool usage events associated with a turn, while ReasoningContent exposes optional model-provided thinking when available. The nullable Content field reflects cases where a turn is tool-only or otherwise without visible text.

## Notes
- VariantGroupId is shared across all siblings in a regeneration group; for non-regenerated turns, VariantGroupId equals Id.
- ToolCallId is populated for messages that involve a tool invocation (tool-role messages); ToolCalls contains the actual tool invocation details when applicable.
- VariantSiblingIds are ordered by CreatedAt to reflect the generation sequence of variants.


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


MessageToolCall is a small, immutable data holder that represents a single tool invocation embedded within a message-based workflow in the Gabriel API. It carries the tool's unique identifier, the tool's logical name, and a JSON string encoding the arguments to pass to that tool. Use this symbol when you need to transport or persist a tool-call intent across system boundaries or construct messages that trigger tool execution, rather than invoking the tool directly.

## Remarks
By encapsulating Id, Name, and ArgumentsJson, this type decouples the transport format from the execution logic. It acts as a lightweight contract that downstream components can interpret to locate and execute the appropriate tool, given the serialized arguments. It fits with other message-related contracts in the API by providing a stable, version-tolerant shape for tool invocations.

## Example
```csharp
var call = new MessageToolCall(
    Id: "tool-translate",
    Name: "TranslateText",
    ArgumentsJson: "{\"text\":\"Hello, world!\",\"to\":\"fr\"}"
);
```

## Notes
- As a record, MessageToolCall is immutable; to change any field, construct a new instance rather than mutating an existing one.
- The ArgumentsJson field is free-form JSON. Ensure it conforms to the expected schema for the target tool to avoid runtime errors during deserialization or execution.

---