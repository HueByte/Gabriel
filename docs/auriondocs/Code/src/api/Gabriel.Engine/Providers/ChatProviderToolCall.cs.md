# ChatProviderToolCall

> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderToolCall.cs`  
> **Kind:** record

```csharp
public record ChatProviderToolCall(string Id, string Name, string ArgumentsJson)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Id` | `string` | — |
| [`Name`](ToolBridge/GabrielToolBridge.cs.md) | `string` | — |
| `ArgumentsJson` | `string` | — |


ChatProviderToolCall is a lightweight, immutable data carrier that represents a single tool invocation issued by the chat provider. It encapsulates the invocation's Id, the tool's Name, and a JSON payload in ArgumentsJson that contains the tool-specific arguments, enabling consistent transport, logging, and correlation across the Gabriel Engine. As a record, it provides value-based equality and immutability, making it suitable for logging, queuing, or persisting tool invocations without coupling to individual tool contracts.

## Remarks
ChatProviderToolCall serves as a boundary between the chat orchestration layer and the tool execution layer. It decouples the notion of 'which tool to call' (Name) from 'what to pass' (ArgumentsJson) and from traceability (Id), enabling a stable, serializable representation across boundaries. Because it is a record, equality is based on its contents, and instances are immutable by design, simplifying testing and deduplication.

## Notes
- ArgumentsJson is opaque to this type; downstream components are responsible for validating and interpreting its structure according to the target tool's contract.
- Id should be unique for each invocation and can serve as a correlation identifier across logs and telemetry.