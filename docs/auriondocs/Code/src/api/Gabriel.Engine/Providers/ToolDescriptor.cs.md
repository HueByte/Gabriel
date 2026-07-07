# ToolDescriptor

> **File:** `src/api/Gabriel.Engine/Providers/ToolDescriptor.cs`  
> **Kind:** record

```csharp
public record ToolDescriptor(string Name, string Description, string ParametersJsonSchema)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| [`Name`](ToolBridge/GabrielToolBridge.cs.md) | `string` | — |
| `Description` | `string` | — |
| `ParametersJsonSchema` | `string` | — |


ToolDescriptor is a provider-facing record that carries metadata about a tool: its Name, a human-readable Description, and a JSON Schema (ParametersJsonSchema) describing the tool's argument shape to the model. It is used when you need to transmit or present tool metadata for invocation without exposing implementation details.

## Remarks
This abstraction decouples tool metadata from its concrete implementation, enabling tooling pipelines to evolve independently. The ParametersJsonSchema enables dynamic generation and validation of inputs, allowing UIs or models to adapt to different tools without code changes.

## Notes
- Ensure the JSON schema string is valid JSON and properly escaped in the C# string literal; invalid JSON or escaping can lead to runtime parsing errors.