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


MessageResponse is an immutable data record that represents a single message within a conversation, capturing its role, content, timing, and its position within a regeneration variant set. It bundles identity (Id), role (e.g., user, assistant, system, tool), content (nullable to accommodate tool-only messages), timestamp (CreatedAt), and the variant-structure metadata (VariantGroupId, VariantIndex, VariantCount, VariantSiblingIds). When a message involves tool invocation, ToolCallId anchors the specific invocation and ToolCalls carries the detailed calls; ReasoningContent is an optional, provider-specific stream of the model's reasoning.

## Remarks
MessageResponse exists to model rich conversational turns where multiple regenerations are possible and where tool usage and reasoning traces may be exposed. By combining the message identity with its variant metadata, downstream components can deterministically re-present, compare, or re-run alternative responses without reconstructing provenance from loose fields. The inclusion of ToolCalls and ReasoningContent supports transparent tool integration and, where supported, reveal the model's reasoning path in a controlled manner.

## Notes
- Content may be null for messages that only involve tool invocations (e.g., an assistant turn that issues tool calls without immediate textual output).
- VariantGroupId is shared across all regen siblings and equals Id when VariantCount is 1 (singleton).
- ToolCalls is non-null only for assistant messages that requested tool usage, and ToolCallId is set on messages with a tool invocation; these two fields should be consistent to reflect the linked tooling activity.

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


MessageToolCall is a minimal data carrier (C# record) used to represent a tool invocation within messaging workflows. It carries a unique identifier (Id), the tool being invoked (Name), and a JSON-encoded payload of arguments (ArgumentsJson). This structure is ideal for passing tool-call metadata across API boundaries or persisting it alongside a message without coupling to the tool's execution logic.

## Remarks

As a record, MessageToolCall provides value through immutability, value-based equality, and simple deconstruction for pattern matching. It decouples the notion of 'which tool' and 'how to execute it' from the surrounding message payload, enabling components to forward or store tool calls without pulling in tool-specific behavior. This abstraction fits into a broader messaging contract by standardizing how tool invocations are represented.

## Notes

- ArgumentsJson must be a valid JSON string that conforms to the expected schema for the target tool; mismatches can cause runtime parsing errors.
- The record is immutable; to create a modified version, use the with-expression (e.g., var c2 = c1 with { Name = "NewName" }).

---