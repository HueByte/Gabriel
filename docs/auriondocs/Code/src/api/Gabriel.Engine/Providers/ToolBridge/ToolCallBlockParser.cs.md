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

Parses <tool_call>...</tool_call> blocks from a buffered text tail (the output produced by ToolCallStreamSplitter) and returns a list of ParsedToolCall instances. Use this when you need to extract tool invocation records emitted by a model message into structured ParsedToolCall objects; the method is deliberately all-or-nothing for a turn — any malformed block causes a ToolCallParseException so caller code (e.g., GabrielToolBridge) can trigger retry behavior.

## Remarks
This internal helper enforces a strict contract: each <tool_call> block must contain a JSON object with a string "name" property and an optional "arguments" object. The parser preserves the raw JSON for "arguments" (serializing the JsonElement back to a JSON string) because downstream consumers expect the raw arguments JSON. It also synthesizes a short, human-identifiable id for each ParsedToolCall (prefixed with "emu_") so tool-call matching works across iterations even when the model does not supply an id.

## Example
```csharp
var text = @"
Some leading text
<tool_call>{""name"": ""X"", ""arguments"": {""foo"": 1}}</tool_call>
intervening text
<tool_call>{""name"": ""Y""}</tool_call>
";

var calls = ToolCallBlockParser.Extract(text);
// calls[0].Name == "X"; calls[0].ArgumentsJson == "{\"foo\":1}";
// calls[1].Name == "Y"; calls[1].ArgumentsJson == "{}";
```

## Notes
- If the input contains an opening <tool_call> without a closing </tool_call>, Extract throws ToolCallParseException and includes the unterminated tail.
- Any JSON problems (missing "name", non-object root, non-string "name", or non-object "arguments") cause a JsonException that Extract wraps in ToolCallParseException; the parser intentionally fails the whole extraction for the turn.
- Missing "arguments" is treated as an empty object and becomes "{}" in the resulting ParsedToolCall.
- The generated id is synthetic (prefix "emu_" + GUID-derived fragment + index) and is intended to avoid collisions within a single turn; it is not a globally stable identifier.


---

## ToolCallParseException

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallBlockParser.cs`  
> **Kind:** class

Thrown when the parser encounters a malformed <tool_call> block in the buffered turn text. Used by GabrielToolBridge to signal that a tool-call block is missing a close tag, contains invalid JSON, or otherwise has the wrong shape; the bridge catches this exception to surface the offending block back to the model and attempt a fix/parse retry.

## Remarks
This exception centralizes parse failures for tool-call blocks and carries the raw (or partial) failing text via the OffendingBlock property so callers can include that snippet in repair requests, logs, or telemetry. It is internal to the provider implementation and intended for control flow where the bridge retries a model response rather than representing a public API parsing error.

## Example
```csharp
try
{
    var parsed = ToolCallBlockParser.Parse(bufferedTurnText);
}
catch (ToolCallParseException ex)
{
    // Send ex.OffendingBlock back to the model to ask for a corrected tool_call block,
    // or log it for debugging/telemetry before retrying.
    logger.LogWarning("Failed to parse tool_call block: {OffendingBlock}", ex.OffendingBlock);
    // GabrielToolBridge will use the offending block when prompting the model to fix its response.
}
```

## Notes
- OffendingBlock may contain a partial block or raw text from the model's response — treat it as untrusted input.  
- Avoid logging or exposing OffendingBlock in environments where model output may contain sensitive data.  
- The type is internal and sealed; it is intended for use inside the tool bridge/parser flow, not as a general-purpose parsing exception.

---

## ParsedToolCall

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallBlockParser.cs`  
> **Kind:** record

Represents a tool call extracted by the parser: a small immutable DTO that carries the tool identifier (Id), the tool's human-readable Name, and the raw JSON string of the call arguments (ArgumentsJson). Use this type when handing parsed calls from the ToolBridge parser to whatever component will validate or execute the tool invocation.

## Remarks
This is an internal, sealed record used as a parsing result inside the tool-bridge plumbing. Being a record it provides value equality, deconstruction, and immutable properties, which makes it convenient to pass between parser and executor without accidental mutation.

## Example
```csharp
// Construct a parsed tool call (typically created by the parser)
var parsed = new ParsedToolCall(
    Id: "tool-123",
    Name: "translate",
    ArgumentsJson: "{\"text\":\"hello\",\"targetLang\":\"es\"}"
);

Console.WriteLine(parsed.Name); // "translate"

// Deconstructing
var (id, name, argsJson) = parsed;
```

## Notes
- ArgumentsJson is a raw JSON string; callers must parse/validate it before use.
- The record is internal and sealed — it's intended for in-assembly use and cannot be extended.
- Properties are non-mutable (immutable), so safe to share across threads if the contained strings are not mutated.

---