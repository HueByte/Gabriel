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


ToolDescriptor is a provider-facing representation of a tool's metadata. It carries the tool's Name, Description, and a JSON Schema describing the shape of its arguments. This record is designed for serialization and transmission to downstream components (for example, prompt-generation or orchestration layers) so tools can be discovered and their inputs validated against a standard interface.

## Remarks
ToolDescriptor decouples tool interface declarations from invocation logic. By storing the parameter shape as a JSON Schema string, it enables dynamic tooling workflows where the same metadata surface can describe diverse tools without changes to the consumer code.

## Notes
- The ParametersJsonSchema must be a valid JSON Schema in string form; consumers rely on it to validate and surface required parameters.
- As a record, ToolDescriptor is immutable and supports "with" expressions to create modified copies.