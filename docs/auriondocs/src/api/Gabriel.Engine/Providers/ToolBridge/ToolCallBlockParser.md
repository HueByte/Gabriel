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

Parses <tool_call>...</tool_call> blocks out of a buffered text tail and returns a list of ParsedToolCall instances. Use this when you have a string produced by ToolCallStreamSplitter (or similar) that may contain one or more serialized tool-call blocks; the parser validates each block's JSON, requires a string 'name' field, accepts an optional 'arguments' object (serialized back to JSON), and synthesizes a deterministic-ish id for each parsed call.

## Remarks
This is a focused, strict extractor: it scans for literal <tool_call> and </tool_call> tags, parses each enclosed JSON body, and fails the entire extraction if any block is malformed. Failures are reported as ToolCallParseException (JSON parsing failures are wrapped so callers can differentiate malformed JSON vs. unterminated tags). The synthesized id (prefixed with "emu_") ensures tool-call identifiers remain unique within a single parsing pass so downstream components can match tool results to calls when messages are re-emitted.

## Example
```csharp
// Example input with two tool_call blocks
string input = "some text <tool_call>{\"name\":\"X\",\"arguments\":{\"a\":1}}</tool_call> trailing <tool_call>{\"name\":\"Y\"}</tool_call> end";
var calls = ToolCallBlockParser.Extract(input);
foreach (var c in calls)
{
    Console.WriteLine($"id={c.Id}, name={c.Name}, args={c.ArgumentsJson}");
}
```

## Notes
- Any unterminated <tool_call> tag (missing closing tag) results in ToolCallParseException containing the tail of the original text.
- JSON inside a block must be an object; the 'name' property must be a string. If present, 'arguments' must be a JSON object.
- Missing 'arguments' is treated as an empty object ("{}") and is returned as a JSON string.
- A single malformed block causes the entire Extract call to throw — parsing is all-or-nothing for the provided input.
- Generated ids are deterministic-ish (a truncated GUID with an index suffix) but are not stable across different runs/inputs; do not rely on them for long-term identity.


---

## ToolCallParseException

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallBlockParser.cs`  
> **Kind:** class

Thrown when the parser encounters a malformed <tool_call> block in buffered turn text. Use this exception to signal that the parser could not produce a valid tool-call structure; the OffendingBlock property contains the raw (or partial) block that failed and is intended to be presented back to the model for correction.

## Remarks
This exception is used by GabrielToolBridge's parsing/retry flow: GabrielToolBridge catches ToolCallParseException to surface the offending text to the model, request a corrected response, and will retry parsing up to GabrielToolBridge.MaxParseRetries before giving up. The class carries the offending input so callers can include it verbatim in a repair prompt or diagnostic log.

## Example
```csharp
// Throwing when a parse fails and you want to include the bad block
if (!TryParseToolCall(blockText, out var result, out var parseError))
{
    throw new ToolCallParseException("Failed to parse tool call block.", blockText, parseError);
}

// Catching and using the offending block for a repair request
try
{
    var call = parser.Parse(blockText);
}
catch (ToolCallParseException ex)
{
    logger.LogWarning("Tool call parse failed: {Message}", ex.Message);
    // Send ex.OffendingBlock back to the model to ask for a corrected tool call
}
```

## Notes
- OffendingBlock may be a partial or truncated fragment; do not assume it contains valid or complete JSON. Inspect or present it as-is for repair rather than attempting further parsing.
- This class is internal and sealed; it is intended for use inside the assembly's GabrielToolBridge parsing/retry logic, not as a public API surface.
- The constructor accepts an inner Exception so callers can preserve underlying parser errors for diagnostics.

---

## ParsedToolCall

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallBlockParser.cs`  
> **Kind:** record

Lightweight, immutable container representing the outcome of parsing a tool-call block. Use this record when the parser needs to return or pass around the parsed pieces of a tool invocation: the Id, the Name, and the raw JSON string of the arguments.

## Remarks
This is an internal, sealed positional record used by the tool-call parsing logic to keep parsed values together with value semantics (structural equality, deconstruction). The ArgumentsJson property contains the argument payload exactly as parsed — consumers are expected to deserialize or validate that JSON as appropriate for their needs.

## Example
```csharp
using System.Text.Json;

var parsed = new ParsedToolCall("tool-123", "translate", "{ \"text\": \"hello\", \"lang\": \"es\" }");

// Deconstruct
var (id, name, argsJson) = parsed;

// Deserialize arguments into a typed object or dictionary
var args = JsonSerializer.Deserialize<Dictionary<string, object?>>(argsJson);

Console.WriteLine(id);   // tool-123
Console.WriteLine(name); // translate
Console.WriteLine(args?["text"]); // hello
```

## Notes
- The record is internal and sealed: it is intended only for use inside the assembly and cannot be inherited.
- ArgumentsJson is stored as raw JSON; it may be null or empty depending on the parser outcome — callers should handle that case and validate JSON before use.
- Being a positional record, it supports deconstruction and value-based equality.

---