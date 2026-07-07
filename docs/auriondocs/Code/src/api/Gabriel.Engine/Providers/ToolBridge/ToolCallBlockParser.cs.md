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


Parses embedded <tool_call> blocks from the text stream produced by ToolCallStreamSplitter and converts each into a structured ParsedToolCall with a synthetic id. Use this when you need to turn a textual stream containing tool invocations into a typed collection that downstream components can feed to tools.

## Remarks
ToolCallBlockParser scans for <tool_call>...</tool_call> blocks, validates that the body is a JSON object with a string 'name' and an optional 'arguments' object, and preserves the raw JSON of the 'arguments'. Missing 'arguments' becomes an empty object ("{}"). Each parsed call is assigned a synthetic, deterministic-ish id of the form emu_<12-char-guid>`_<index>`, intended to help correlation within a single turn while clearly signaling synthetic origin. If a block is unterminated or the JSON inside cannot be parsed, a ToolCallParseException is thrown to drive the retry flow and surface a precise diagnostic. This parser deliberately does not attempt to interpret the tool name or arguments beyond validation and raw serialization, leaving execution to downstream collaborators.

## Example
```csharp
// Basic extraction from a string with a single block
var text = "<tool_call> {\"name\":\"echo\",\"arguments\":{\"text\":\"hi\"}} </tool_call>";
var calls = ToolCallBlockParser.Extract(text);
```

## Notes
- Unterminated <tool_call> blocks cause a ToolCallParseException with a descriptive message to aid retry logic.
- If 'arguments' is present, it must be a JSON object; otherwise a JsonException is thrown and wrapped in ToolCallParseException.
- When 'arguments' is absent, the parser uses "{}" to allow tools with no parameters to run.


---

## ToolCallParseException
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallBlockParser.cs`  
> **Kind:** class

```csharp
internal sealed class ToolCallParseException : Exception
```


Thrown when at least one <tool_call> block in the buffered turn text is malformed (missing close tag, invalid JSON, or incorrect shape). It exposes the exact offending block via the OffendingBlock property so the fix-up mechanism can prompt the model to repair it and retry parsing, up to GabrielToolBridge.MaxParseRetries attempts.

## Remarks

ToolCallParseException is a focused error type that travels the parse failure from the parser to the repair orchestrator without leaking implementation details. The OffendingBlock property makes the failure actionable by enabling precise repair prompts and diagnostics. The class is internal and sealed to emphasize its role as a simple, immutable signal in the error-handling pipeline, while the optionally chained inner exception preserves the original parsing error for debugging.

## Notes

- The OffendingBlock may contain large payloads or sensitive content; avoid logging or displaying it in user-visible channels without sanitization.
- This exception participates in a retry policy governed by GabrielToolBridge.MaxParseRetries; changing that value alters how many times a malformed block is repaired and retried.


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


It is an immutable record that captures the essential components of a parsed tool invocation: Id, Name, and ArgumentsJson. It serves as a compact, transportable snapshot used within the ToolBridge parsing pipeline to carry a single parsed call from the parser toward execution, while preserving the raw JSON payload for arguments.

## Remarks
Because it uses a record, ParsedToolCall benefits from value-based equality, deconstruction, and concise equality semantics, which simplifies testing and pattern matching on parsed calls. The internal sealed accessibility reinforces the boundary of the parsing layer, ensuring this detail remains encapsulated within the ToolBridge implementation. Storing ArgumentsJson as a JSON string decouples the call's argument shape from this type, enabling evolution of argument schemas without changing the record's surface.

## Notes
- ArgumentsJson is a raw JSON payload; deserialise it at the point of use with proper validation to avoid security or runtime issues.
- Id and Name act as routing and correlation metadata; this type does not enforce any mapping guarantees beyond carrying those values as parsed from the source.

---