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


Represents an immutable data container for a chat provider's tool invocation. It captures the call's identity (Id), the target tool's name (Name), and the JSON-encoded arguments for that tool (ArgumentsJson). This is a lightweight value object intended for passing tool-call metadata through the chat/provider pipeline in a strongly-typed, copy-friendly manner.

## Remarks
This type serves as a small, dedicated payload that decouples the notion of a tool call from the surrounding orchestration logic. By virtue of being a record, it provides value-based equality and convenient deconstruction, which simplifies comparisons, storage, and signaling of identical tool invocations across components. It centralizes the shape of a tool call, making it easy to serialize, log, or route without exposing internal dispatch details elsewhere in the system.