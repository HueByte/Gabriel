# ChatProviderToolCall

> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderToolCall.cs`  
> **Kind:** record

Represents a single tool invocation requested by the assistant: an opaque identifier, the tool's name, and the raw JSON string of arguments the model emitted. Use this record when capturing or forwarding a tool call from a chat provider to whatever component is responsible for executing or validating the tool invocation.

## Remarks
This is a small immutable data carrier (positional record) intended to transport a model-generated tool call between components. It intentionally stores ArgumentsJson as a raw JSON string — parsing, validation, and any mapping from that JSON into typed arguments are responsibilities of the tool executor. The record provides value-based equality and deconstruction semantics typical of C# records.

## Example
```csharp
// Create a call from a model and forward it to a hypothetical executor
var call = new ChatProviderToolCall("call-001", "web.search", "{ \"query\": \"cats\" }");
executor.Execute(call.Name, call.ArgumentsJson);
```

## Notes
- ArgumentsJson is stored without validation; ensure the executor validates or parses it safely before use.
- The Id field is an opaque identifier used by callers; the type is string (not enforced as a GUID).
- As a record, instances are immutable and compare by value for the three properties.