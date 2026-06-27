# MessageResponse.cs

> **Source:** `src/api/Gabriel.API/Contracts/Messages/MessageResponse.cs`

## Contents

- [MessageResponse](#messageresponse)
- [MessageToolCall](#messagetoolcall)

---

## MessageResponse

> **File:** `src/api/Gabriel.API/Contracts/Messages/MessageResponse.cs`  
> **Kind:** record

A compact immutable representation of a single chat message returned by the API, including metadata used for message regeneration/variant tracking, tool invocations, and optional model "thinking" (reasoning) output. Reach for this record when consuming or storing conversation turns from the service — it encodes both the message payload and the contextual bookkeeping (variant groups, sibling ordering, tool call linkage) that higher-level consumers need.

## Remarks
This record groups together several responsibilities that are commonly required by clients and storage layers: identifying messages (Id), tracking creation time (CreatedAt), representing the role that produced the message (Role), and carrying the message text (Content). It also models regenerated messages as a variant group: VariantGroupId is shared by all siblings produced by regeneration, VariantIndex is the zero-based position of this message among its siblings (ordered by CreatedAt), VariantCount is the total siblings in the group, and VariantSiblingIds lists all sibling Ids in CreatedAt order (and includes this message's Id). Tool-related fields represent either a tool invocation (ToolCallId present on tool-role messages) or assistant-originated tool call requests/responses (ToolCalls present on assistant messages). ReasoningContent is an optional stream of model internal reasoning when supported by the provider.

## Example
```csharp
// Assistant message that requested tool calls and produced optional reasoning
var assistantMsg = new MessageResponse(
    Id: Guid.NewGuid(),
    Role: "assistant",
    Content: "I found these results; invoking tool X for details.",
    CreatedAt: DateTimeOffset.UtcNow,
    VariantGroupId: Guid.NewGuid(),
    VariantIndex: 0,
    VariantCount: 1,
    VariantSiblingIds: new List<Guid> { /* self id */ }.AsReadOnly(),
    ToolCallId: null,
    ToolCalls: new List<MessageToolCall> { /* tool call records */ }.AsReadOnly(),
    ReasoningContent: "{" + "step1..." + "}"
);

// Tool-produced message (e.g. output from a tool invoked by the assistant)
var toolMsg = new MessageResponse(
    Id: Guid.NewGuid(),
    Role: "tool",
    Content: "Result from tool",
    CreatedAt: DateTimeOffset.UtcNow,
    VariantGroupId: Guid.NewGuid(),
    VariantIndex: 0,
    VariantCount: 1,
    VariantSiblingIds: new List<Guid> { /* self id */ }.AsReadOnly(),
    ToolCallId: "tool-call-123",
    ToolCalls: null,
    ReasoningContent: null
);
```

## Notes
- Content can be null for messages that contain no textual payload (for example, an assistant turn that only produced tool calls).
- VariantIndex is zero-based and computed by ordering siblings by CreatedAt; ties in CreatedAt may affect index assignment.
- VariantGroupId equals Id for non-regenerated (singleton) messages.
- VariantSiblingIds is an IReadOnlyList that includes this message's Id and is ordered by CreatedAt; the underlying collection may still be mutable if constructed that way, so do not rely on deep immutability beyond the IReadOnlyList contract.
- ToolCallId is populated for messages with Role == "tool"; ToolCalls is populated for assistant messages that requested or reported tool activity.
- ReasoningContent may represent an incremental/streamed thinking output from reasoning-capable providers and can be null when unsupported or not produced.


---

## MessageToolCall

> **File:** `src/api/Gabriel.API/Contracts/Messages/MessageResponse.cs`  
> **Kind:** record

Represents a single tool invocation attached to a message response: an opaque identifier for the call, the tool's name, and the tool arguments encoded as a JSON string. Use this record when you need to serialize or transport a tool call (its identity, which tool to run, and the JSON payload of arguments) between components or across process boundaries.

## Remarks
This is a compact, immutable DTO (record) intended for inter-component communication or for inclusion in API responses. It captures only the minimal information required to identify and replay a tool call; actual execution logic is kept separate. The record uses value-based equality, making instances easy to compare in tests or when deduplicating calls.

## Example
```csharp
// Constructing a tool call with JSON-encoded arguments
var argsJson = System.Text.Json.JsonSerializer.Serialize(new { text = "hello", target = "es" });
var call = new MessageToolCall("call-123", "translate", argsJson);

// Serializing the record for transmission
var serialized = System.Text.Json.JsonSerializer.Serialize(call);

// Deserializing back
var deserialized = System.Text.Json.JsonSerializer.Deserialize<MessageToolCall>(serialized);
```

## Notes
- ArgumentsJson is expected to be a JSON string containing the tool's arguments. Avoid double-serializing (i.e., don't serialize an already-serialized JSON string again).
- Id is an opaque identifier for the call; its format and uniqueness are determined by the producer.
- As a record, instances are immutable and compare by value, which is useful for testing and de-duplication.

---