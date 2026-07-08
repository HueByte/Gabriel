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


ToolCallBlockParser extracts all <tool_call> blocks from a buffered text tail and converts each into a ParsedToolCall, assigning a deterministic synthetic id to preserve cross-turn correlation. Use it when the model's output interleaves narrative with tool invocations so you can validate, summarize, and dispatch each call to the tool layer.

## Remarks
Its parsing is centralized and strict: it requires a closing </tool_call> tag for every opening <tool_call> and validates that the body is valid JSON with a string 'name' field and an object 'arguments' (which may be omitted, defaulting to {})). Any parse failure raises ToolCallParseException, which triggers the surrounding retry loop. The synthetic id (prefixed with emu_) helps correlate results across the turn, making tool_call_id matching reliable in transport layers. The implementation also normalizes missing arguments to an empty object, so tools can be invoked without requiring explicit arguments.

## Example
```csharp
string text = "<tool_call>{\"name\":\"Echo\",\"arguments\":{\"text\":\"hello\"}}</tool_call>";
var calls = ToolCallBlockParser.Extract(text);
foreach (var call in calls)
{
    Console.WriteLine($"{call.Id} - {call.Name} - {call.ArgumentsJson}");
}
```

## Notes
- Unterminated <tool_call> blocks throw ToolCallParseException with a descriptive message.
- Missing or non-string 'name' results in a JsonException describing the requirement.
- The generated id is synthetic and unique per turn, including an index suffix to avoid collisions.

---

## ToolCallParseException
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallBlockParser.cs`  
> **Kind:** class

```csharp
internal sealed class ToolCallParseException : Exception
```


Thrown when at least one <tool_call> block in the buffered turn text was malformed (missing close tag, invalid JSON, wrong shape). GabrielToolBridge catches this to ask the model to fix its previous response and retries up to GabrielToolBridge.MaxParseRetries times before giving up. The exception carries the offendingBlock via the OffendingBlock property so the repair process can quote the exact fragment back to the model and guide the repair.

## Remarks
The ToolCallParseException represents a focused failure mode for tool invocation parsing. It isolates parsing errors from the normal control flow, enabling a targeted repair/retry cycle around tool blocks. By preserving the exact offending fragment, it provides precise context for the repair step while keeping surrounding state intact. The internal sealed nature confines this error pathway to the Gabriel.Engine tooling surface, ensuring a single, consistent handling strategy.

## Notes
- The OffendingBlock content may be large or sensitive; avoid exposing it in user-facing logs or messages beyond what is necessary for repair.
- If an inner exception is supplied, preserve it to retain the full debugging trail.
- This exception is specific to the tool-block parser and is not intended for general-purpose parsing errors in other subsystems.

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


ParsedToolCall is an immutable, value-based carrier that captures a single parsed tool invocation. It stores an identifier, the tool name, and a JSON string of arguments produced by the parsing stage.

## Remarks
Because it is a sealed record, it provides structural equality and convenient deconstruction, making it easy to compare calls or extract the three components in a pattern-friendly way. It serves as a stable data contract between the ToolCallBlockParser and downstream components that execute or log tool invocations, ensuring callers aren’t coupled to parsing internals.

## Example
```csharp
// Example: constructing and inspecting a parsed tool call
var call = new ParsedToolCall("call-42", "BuildProject", "{\"project\":\"src\"}");

// Access members directly
string id = call.Id;
string name = call.Name;
string args = call.ArgumentsJson;

// Or deconstruct
var (deconstructedId, deconstructedName, deconstructedArgs) = call;
```

## Notes
- ArgumentsJson is a JSON-encoded string; no validation is performed by this type.
- The type is internal to its assembly, so usage is intended within the ToolBridge parsing/execution flow.
- Equality on ParsedToolCall is value-based (by Id, Name, and ArgumentsJson) due to it being a record.

---