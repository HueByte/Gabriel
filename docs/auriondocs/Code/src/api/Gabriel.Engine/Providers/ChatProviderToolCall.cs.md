# ChatProviderToolCall

> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderToolCall.cs`  
> **Kind:** record

Represents a single tool invocation requested by the assistant, carrying a unique identifier (Id), the tool's name (Name), and the tool call arguments as a raw JSON string (ArgumentsJson). Reach for this record when you need a compact, immutable DTO to pass, log, or persist tool-call metadata between components (executor, scheduler, logger) rather than embedding complex typed objects.

## Remarks
This is a lightweight, immutable data container — a serialization-friendly boundary object that intentionally stores arguments as raw JSON to avoid premature or repeated deserialization. It relies on callers/consumers to parse and validate ArgumentsJson; the record does not perform any transformation or schema enforcement. Being a C# record, it provides value-based equality and a concise syntax for creation and pattern matching, which is useful in tests and message-passing scenarios.

## Example
```csharp
using System.Text.Json;

var call = new ChatProviderToolCall(
    Id: "call-001",
    Name: "translate",
    ArgumentsJson: "{\"text\":\"hello\",\"to\":\"es\"}"
);

// Parse arguments when needed
using var doc = JsonDocument.Parse(call.ArgumentsJson);
var root = doc.RootElement;
string text = root.GetProperty("text").GetString();
string to = root.GetProperty("to").GetString();

// Or deserialize into a POCO
var args = JsonSerializer.Deserialize<TranslateArgs>(call.ArgumentsJson);
```

## Notes
- ArgumentsJson is stored as raw JSON; consumers must parse and validate it before use.
- The record does not enforce uniqueness or format of Id — any required uniqueness must be enforced by the caller or surrounding infrastructure.
- As an immutable record, instances are value-equal; create a new instance to represent a different invocation.