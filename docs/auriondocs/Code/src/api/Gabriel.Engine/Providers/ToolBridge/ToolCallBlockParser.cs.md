# ToolCallBlockParser.cs

> **Source:** `src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallBlockParser.cs`

## Contents

- [ToolCallBlockParser](#toolcallblockparser)
- [ToolCallParseException](#toolcallparseexception)
- [ParsedToolCall](#parsedtoolcall)

---

## ToolCallBlockParser
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallBlockParser.cs`  
> **Kind:** class

```csharp
internal static class ToolCallBlockParser
```


ToolCallBlockParser scans a text tail for <tool_call> blocks emitted by the tool bridge, validates each block, and converts them into ParsedToolCall entries containing an id, a name, and a raw JSON payload of arguments. The extraction is all-or-nothing per turn: a single malformed block causes the whole extraction to throw ToolCallParseException to drive the retry loop.

## Remarks
The id on each ParsedToolCall is synthesized locally to maintain a stable mapping for downstream state tracking (AgentContext tool_call_id matching) across the round-trips of tool execution. The implementation uses a deterministic-ish generation pattern that prefixes with emu_ and appends a short GUID-derived segment plus the block index, ensuring uniqueness within a turn while signaling its origin in logs and persistence.

## Notes
- Unterminated <tool_call> blocks raise a ToolCallParseException with context, forcing a retry.
- If the block contains an 'arguments' value, it must be an object; otherwise a JsonException is raised and wrapped as ToolCallParseException.
- When 'arguments' is absent, {} is used as the default payload, allowing tools with no parameters to execute.
- The extraction is performed by a straightforward scan for the OpenTag and CloseTag markers; blocks are processed in order and accumulated into a single `IReadOnlyList<ParsedToolCall>` unless an error occurs.


---

## ToolCallParseException
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallBlockParser.cs`  
> **Kind:** class

```csharp
internal sealed class ToolCallParseException : Exception
```


ToolCallParseException is an internal sealed exception that signals a malformed tool_call block found in the buffered turn text. It is thrown when a block is missing a closing tag, contains invalid JSON, or does not conform to the expected shape, and it is caught by GabrielToolBridge to prompt the model to repair the previous response and retry up to GabrielToolBridge.MaxParseRetries times before giving up. The exception carries OffendingBlock, the raw offending fragment (or partial block), which is echoed back to the model in the fix-up message to guide the repair.

## Remarks
By isolating parse errors into a dedicated exception type, the system cleanly separates parsing concerns from normal control flow and enables a targeted retry boundary. OffendingBlock provides actionable context to the repair step, ensuring the model sees the exact fragment that failed, which improves the chances of a correct repair on subsequent attempts.

## Notes
- OffendingBlock may be the entire failed block or a partial fragment, so the repair prompt should not assume a complete, well-formed piece of JSON.
- The class is internal to the assembly and not part of the public API.
- The exception's state is limited to the OffendingBlock property (in addition to the standard Message and InnerException).

---

## ParsedToolCall
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallBlockParser.cs`  
> **Kind:** record

```csharp
internal sealed record ParsedToolCall(string Id, string Name, string ArgumentsJson)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Id` | `string` | — |
| [`Name`](GabrielToolBridge.cs.md) | `string` | — |
| `ArgumentsJson` | `string` | — |


ParsedToolCall is an internal, immutable data carrier that encapsulates the result of parsing a tool invocation within the ToolBridge workflow. It stores the call's identifier (Id), the tool name (Name), and a JSON string of the arguments (ArgumentsJson), enabling downstream components to treat a parsed invocation as a single, passable unit.

## Remarks
This record acts as a lightweight boundary between the parsing layer and the execution/dispatch logic. Its value-based equality and deconstruction support straightforward testing and convenient access to individual fields. Because it is internal, its shape is a constraint of the ToolBridge pipeline rather than a public contract. The ArgumentsJson field provides a flexible, schema-less representation of parameters that can be consumed by JSON parsers when needed.

## Example
```csharp
var call = new ParsedToolCall("42", "RunAnalysis", "{\"dataset\":\"sales\",\"metrics\":[\"revenue\",\"growth\"]}");
(string id, string name, string argumentsJson) = call; // deconstructs the record
```

## Notes
- ArgumentsJson must be well-formed JSON; invalid JSON may surface as a failure downstream.
- Id should be treated as an opaque identifier for the parsed call; do not rely on its structure.
- Since ParsedToolCall is a record, you can create a modified copy with a with-expression if needed (e.g., var updated = call with { Id = newId }).

---